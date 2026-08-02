import { render } from "solid-js/web";
import { Router, Route } from "@solidjs/router";
import { ThemeProvider } from "@supercluster/core";
import { lazy, type Component, type JSX } from "solid-js";
import "./styles.css";

import AuthLayout from "./layouts/AuthLayout";
import DashboardLayout from "./layouts/DashboardLayout";

const AppShell: Component<{ children: JSX.Element }> = (props) => (
  <div class="min-h-full bg-surface">{props.children}</div>
);

const root = document.getElementById("root");
if (!root) throw new Error("Root element not found");

render(
  () => (
    <ThemeProvider>
      <AppShell>
        <Router>
          <Route path="/auth" component={AuthLayout}>
            <Route path="/login" component={lazy(() => import("./pages/auth/Login"))} />
            <Route path="/register" component={lazy(() => import("./pages/auth/Register"))} />
            <Route path="/verify" component={lazy(() => import("./pages/auth/VerifyEmail"))} />
            <Route path="/recovery" component={lazy(() => import("./pages/auth/Recovery"))} />
            <Route path="/unsupported" component={lazy(() => import("./pages/auth/Unsupported"))} />
          </Route>
          <Route path="/dashboard" component={DashboardLayout}>
            <Route path="/" component={lazy(() => import("./pages/Dashboard"))} />
          </Route>
        </Router>
      </AppShell>
    </ThemeProvider>
  ),
  root,
);