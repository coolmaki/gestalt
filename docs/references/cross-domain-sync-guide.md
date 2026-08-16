# Cross-Domain Front-Channel Data Sync Implementation Guide

This document outlines the architecture, code implementation, and production considerations for syncing data instantly across different domains (e.g., `site-a.com` to `site-b.com`) entirely within the user's browser (front-channel).

## 1. Architectural Overview
Modern browser sandboxing implements the Same-Origin Policy, restricting direct communication or shared storage APIs (like `BroadcastChannel` or `localStorage`) across different domains. 

To securely bypass this limitation in the front-channel, a **Hub-and-Spoke Architecture** is utilized:
* **The Central Hub (`central-hub.com`)**: A designated central domain hosting a hidden page that acts as the master storage repository.
* **The Spokes (`site-a.com`, `site-b.com`)**: Independent partner domains that embed the Central Hub via a hidden `<iframe>` and pass data using HTML5 Web Messaging (`window.postMessage`).

```
+-------------------------------------------------------------------+
|                           User Browser                            |
|                                                                   |
|   +-----------------------+           +-----------------------+   |
|   |  Spoke: site-a.com    |           |  Spoke: site-b.com    |   |
|   |                       |           |                       |   |
|   |  +-----------------+  |           |  +-----------------+  |   |
|   |  |   Hidden iframe |  |           |  |   Hidden iframe |  |   |
|   |  | central-hub.com |  |           |  | central-hub.com |  |   |
|   +--+--------+--------+--+           +--+--------+--------+--+   |
|               |                                   ^               |
|  postMessage  | (WRITE)                           | postMessage   |
|  (New Data)   v                                   | (Broadcast)   |
|   +-----------+-----------------------------------+-----------+   |
|   |               Central Hub (Shared Storage Engine)         |   |
|   |                      localStorage / IndexedDB             |   |
|   +-----------------------------------------------------------+   |
+-------------------------------------------------------------------+
```

---

## 2. Core Implementation Code

### 2.1 Central Hub (`central-hub.com/sync.html`)
This file is hosted on your central domain. It accepts requests only from explicitly whitelisted origins, reads/writes data to its own storage, and broadcasts state updates.

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Storage Hub</title>
</head>
<body>
<script>
  // Explicitly whitelist trusted partner sites allowed to sync data
  const ALLOWED_ORIGINS = [
    'https://site-a.com',
    'https://site-b.com'
  ];

  // List of connected tabs/windows to broadcast changes back to
  const connectedSpokes = new Set();

  window.addEventListener('message', (event) => {
    // 1. Strict Security: Reject messages from unauthorized origins
    if (!ALLOWED_ORIGINS.includes(event.origin)) {
      console.warn(`Rejected unauthorized access attempt from: ${event.origin}`);
      return;
    }

    const { action, key, value } = event.data;

    // Track the sender source for broadcasting purposes
    if (event.source && !connectedSpokes.has(event.source)) {
      connectedSpokes.add(event.source);
    }

    switch (action) {
      case 'WRITE':
        try {
          // 2. Persist data into the hub's domain storage
          localStorage.setItem(key, JSON.stringify(value));
          
          // 3. Acknowledge success to the sender
          event.source.postMessage({ status: 'WRITE_SUCCESS', key }, event.origin);
          
          // 4. Proactively broadcast the change to all other listening spoke windows
          broadcastChange(event.origin, { action: 'SYNC_UPDATE', key, value });
        } catch (error) {
          event.source.postMessage({ status: 'ERROR', message: error.message }, event.origin);
        }
        break;

      case 'READ':
        const storedData = localStorage.getItem(key);
        event.source.postMessage({ 
          status: 'READ_REPLY', 
          key, 
          value: storedData ? JSON.parse(storedData) : null 
        }, event.origin);
        break;
        
      default:
        console.warn('Unknown sync action received:', action);
    }
  });

  function broadcastChange(senderOrigin, payload) {
    // Distribute data update to all actively communicating spokes (excluding original sender if desired)
    connectedSpokes.forEach(spokeWindow => {
      try {
        // Since we don't know each exact window's origin mapping offhand in the Set,
        // we can broadcast safely by targeting the known allowed origins.
        ALLOWED_ORIGINS.forEach(origin => {
          spokeWindow.postMessage(payload, origin);
        });
      } catch (e) {
        // Clean up stale or closed window references
        connectedSpokes.delete(spokeWindow);
      }
    });
  }
</script>
</body>
</html>
```

### 2.2 Spoke Client Implementation (`site-a.com` or `site-b.com`)
Include this script within your client web applications to establish a pipeline to the Central Hub.

```html
<!-- Embedded hidden hub iframe -->
<iframe id="sync-hub" src="https://central-hub.com/sync.html" style="display:none; visibility:hidden;" width="0" height="0"></iframe>

<script>
  const hubFrame = document.getElementById('sync-hub');
  const HUB_ORIGIN = 'https://central-hub.com';
  let isHubReady = false;

  // Wait for the iframe to load before executing data operations
  hubFrame.addEventListener('load', () => {
    isHubReady = true;
    console.log('Sync Hub iframe successfully established.');
    
    // Automatically pull latest data on startup
    requestDataFromHub('user_session');
  });

  // API: Send an update to the Central Hub
  function syncDataToHub(key, data) {
    if (!isHubReady) {
      console.error('Cannot sync: Hub iframe is not fully loaded.');
      return;
    }
    hubFrame.contentWindow.postMessage({ action: 'WRITE', key, value: data }, HUB_ORIGIN);
  }

  // API: Request data from the Central Hub
  function requestDataFromHub(key) {
    if (!isHubReady) return;
    hubFrame.contentWindow.postMessage({ action: 'READ', key }, HUB_ORIGIN);
  }

  // Pipeline Event Listener: Incoming communications from the Central Hub
  window.addEventListener('message', (event) => {
    // Strict Security Check: Enforce origin matches the trusted hub
    if (event.origin !== HUB_ORIGIN) return;
    
    const { status, action, key, value } = event.data;

    // Handle initial read replies
    if (status === 'READ_REPLY') {
      handleStateUpdate(key, value);
    }

    // Handle realtime broadcast updates triggered by other tabs or domains
    if (action === 'SYNC_UPDATE') {
      console.log(`Realtime front-channel update received for [${key}]`);
      handleStateUpdate(key, value);
    }
  });

  // Application Logic Layer
  function handleStateUpdate(key, value) {
    console.log('App state synchronized:', key, value);
    // Execute local UI updates, state container updates (Redux, Vuex, etc.), or context changes here
  }
</script>
```

---

## 3. Production Barriers & Mitigations

### 3.1 Storage Partitioning (CHIPS / ITP)
Modern browser privacy architectures—such as Apple's Intelligent Tracking Prevention (ITP) and Google Chrome's third-party cookie phase-out—partition storage for third-party context iframes. 

* **The Issue:** When `central-hub.com` is loaded inside an iframe on `site-a.com`, the browser wraps its `localStorage` inside a sandbox tied exclusively to `site-a.com`. When loaded on `site-b.com`, it sees a separate sandbox, preventing true cross-domain synchronization.
* **Mitigation Strategy:**
  1. **Storage Access API (SAA):** Call `document.requestStorageAccess()` within the iframe to prompt the user to allow unpartitioned storage access.
  2. **First-Party CNAME Cloaking:** If domains belong to the same parent entity, use DNS routes (e.g., `hub.site-a.com` and `hub.site-b.com`) to allow first-party treatment, though cross-site validation is still strictly enforced.

### 3.2 Race Conditions
If multiple domains attempt to update the same key at the exact same millisecond, state drift can occur.

* **Mitigation Strategy:** Implement vector clocks, monotonic sequence numbers, or timestamps within your payload (`value: { data: ..., updatedAt: Date.now() }`). Ensure the Central Hub evaluates this metadata before overwriting records.

### 3.3 Security & XSS Vector Prevention
Improper implementation creates major security vulnerabilities, allowing attackers to hijack user sessions.

* **Mitigation Rules:**
  1. **Never use wildcard targets (`*`):** Always specify explicit origins in `postMessage(data, targetOrigin)`.
  2. **Sanitize Data:** Treat incoming data payloads into the client as untrusted user inputs. Sanitize strings before parsing or binding to the DOM to prevent Cross-Site Scripting (XSS).
  3. **Content Security Policy (CSP):** Apply a strict CSP header to the `sync.html` deployment file to explicitly restrict execution parameters:
     ```http
     Content-Security-Policy: default-src 'none'; script-src 'unsafe-inline'; frame-ancestors https://site-a.com https://site-b.com;
     ```