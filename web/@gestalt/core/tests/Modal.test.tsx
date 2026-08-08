import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { Modal } from "@/components/Modal";

describe("Modal", () => {
  it("renders when open is true", () => {
    render(() => (
      <Modal open={true} onClose={() => {}} title="Test Modal">
        <p>Modal content</p>
      </Modal>
    ));
    expect(screen.getByText("Test Modal")).toBeTruthy();
    expect(screen.getByText("Modal content")).toBeTruthy();
  });

  it("does not render when open is false", () => {
    render(() => (
      <Modal open={false} onClose={() => {}}>
        <p>Hidden</p>
      </Modal>
    ));
    expect(screen.queryByText("Hidden")).toBeNull();
  });

  it("calls onClose when close button is clicked", () => {
    let closed = false;
    render(() => (
      <Modal open={true} onClose={() => { closed = true; }}>
        <p>Content</p>
      </Modal>
    ));
    const closeButton = document.querySelector("button[aria-label='Close']") as HTMLButtonElement;
    closeButton.click();
    expect(closed).toBe(true);
  });

  it("calls onClose when backdrop is clicked", () => {
    let closed = false;
    render(() => (
      <Modal open={true} onClose={() => { closed = true; }}>
        <p>Content</p>
      </Modal>
    ));
    const backdrop = document.querySelector(".bg-black\\/60") as HTMLElement;
    backdrop.click();
    expect(closed).toBe(true);
  });

  it("sets aria-modal and role", () => {
    render(() => (
      <Modal open={true} onClose={() => {}}>
        <p>Content</p>
      </Modal>
    ));
    expect(document.querySelector("[role='dialog']")).toBeTruthy();
    expect(document.querySelector("[aria-modal='true']")).toBeTruthy();
  });
});