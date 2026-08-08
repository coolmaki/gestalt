import { describe, it, expect } from "vitest";
import { render, screen } from "@solidjs/testing-library";
import { Text } from "@/components/Text";

describe("Text", () => {
  it("renders body text by default", () => {
    render(() => <Text>Hello world</Text>);
    expect(screen.getByText("Hello world").tagName).toBe("P");
  });

  it("renders headline as h2", () => {
    render(() => <Text variant="headline">Title</Text>);
    expect(screen.getByText("Title").tagName).toBe("H2");
  });

  it("renders subhead as h3", () => {
    render(() => <Text variant="subhead">Subtitle</Text>);
    expect(screen.getByText("Subtitle").tagName).toBe("H3");
  });

  it("renders caption as span", () => {
    render(() => <Text variant="caption">Caption</Text>);
    expect(screen.getByText("Caption").tagName).toBe("SPAN");
  });

  it("overrides HTML tag with as prop", () => {
    render(() => <Text variant="body" as="h1">Big body</Text>);
    expect(screen.getByText("Big body").tagName).toBe("H1");
  });

  it("applies custom class", () => {
    render(() => <Text class="my-class">Text</Text>);
    expect(screen.getByText("Text").classList.contains("my-class")).toBe(true);
  });
});