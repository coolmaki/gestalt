import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { FormField } from "@/components/FormField";

describe("FormField", () => {
  it("renders label and children", () => {
    render(() => (
      <FormField label="Email" htmlFor="email">
        <input id="email" />
      </FormField>
    ));
    expect(screen.getByText("Email")).toBeTruthy();
    expect(screen.getByLabelText("Email")).toBeTruthy();
  });

  it("renders error message when provided", () => {
    render(() => (
      <FormField label="Email" htmlFor="email" error="Email is required">
        <input id="email" />
      </FormField>
    ));
    expect(screen.getByText("Email is required")).toBeTruthy();
  });

  it("does not render error when not provided", () => {
    render(() => (
      <FormField label="Name" htmlFor="name">
        <input id="name" />
      </FormField>
    ));
    expect(document.querySelectorAll("text-danger").length).toBe(0);
  });

  it("renders with custom class", () => {
    render(() => (
      <FormField label="Name" htmlFor="name" class="my-class">
        <input id="name" />
      </FormField>
    ));
    expect(document.querySelector(".my-class")).toBeTruthy();
  });
});