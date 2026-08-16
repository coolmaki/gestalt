import type { Component } from "solid-js";
import { createResource, For } from "solid-js";
import { ICON_VIEWBOX, type IconName, type IconShape } from "./icon-data";
import { loadIconShapes } from "./registry";

export interface IconProps {
  name: IconName;
  size?: number;
  class?: string;
}

export { type IconName, Icons } from "./icon-data";

function renderShape(shape: IconShape) {
  switch (shape.type) {
    case "path":
      return <path d={shape.d} />;
    case "rect":
      return (
        <rect
          x={shape.x}
          y={shape.y}
          width={shape.width}
          height={shape.height}
          rx={shape.rx}
        />
      );
    case "ellipse":
      return <ellipse cx={shape.cx} cy={shape.cy} rx={shape.rx} ry={shape.ry} />;
  }
}

export const Icon: Component<IconProps> = (props) => {
  const [shapes] = createResource(() => props.name, loadIconShapes);

  const sz = () => props.size ?? 24;
  const rem = () => sz() / 16;

  return (
    <span
      class={`inline-flex items-center justify-center shrink-0 ${props.class ?? ""}`}
      style={{ width: `${rem()}rem`, height: `${rem()}rem` }}
      aria-hidden="true"
    >
      {shapes() && (
        <svg viewBox={ICON_VIEWBOX} fill="currentColor" width={sz()} height={sz()}>
          <For each={shapes()}>{renderShape}</For>
        </svg>
      )}
    </span>
  );
};
