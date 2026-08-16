import { createSignal } from "solid-js";
import { Modal } from "@/components/Modal";
import { Button } from "@/components/Button";
import { Card } from "@/components/Card";
import { Text } from "@/components/Text";
import { Input } from "@/components/Input";
import { FormField } from "@/components/FormField";

export function ModalDemo() {
  const [open, setOpen] = createSignal(false);
  const [title, setTitle] = createSignal("Confirm Action");

  return (
    <div>
      <Text variant="headline" class="mb-6">Modal</Text>
      <div class="grid grid-cols-[1fr_280px] gap-6">
        <Card>
          <div class="flex items-center justify-center p-12">
            <Button variant="primary" onClick={() => setOpen(true)}>Open Modal</Button>
          </div>
        </Card>
        <Card>
          <div class="flex flex-col gap-4 p-4">
            <FormField label="Title" htmlFor="modal-title">
              <Input
                id="modal-title"
                value={title()}
                onChange={setTitle}
              />
            </FormField>
          </div>
        </Card>
      </div>
      <Modal open={open()} onClose={() => setOpen(false)} title={title()}>
        <div class="flex flex-col gap-4">
          <p class="text-medium-emphasis">
            This modal is centered on the page with a backdrop blur.
            Tab through the interactive elements to test the focus trap.
          </p>
          <div class="flex justify-end gap-3">
            <Button variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button variant="danger" onClick={() => setOpen(false)}>Delete</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}