import { ajaxPost } from "../utilities.js";

export function handleFormSubmit(form, onSave = null) {
    const objectId = form.querySelector('.object-id');
    const inputs = form.querySelectorAll('.form-input');
    const saveBtn = form.querySelector('.form-submit');
    const saveUrl = form.getAttribute('action');

    saveBtn.addEventListener('click', () => {
        ajaxPost(saveUrl, createFormValues(objectId, inputs), (data) => {
            saveBtn.disabled = true;
            onSave?.(data);
        });
    })
}

// TODO: Could probably use FormData instead of this.
export function createFormValues(objectIdInput, inputs) {
    const obj = {};
    if (objectIdInput) {
        obj[objectIdInput.name] = objectIdInput.value;
    }

    inputs.forEach(input => {
        obj[input.name] = input.type === 'checkbox' ? input.checked : input.value;
    });

    return obj;
}