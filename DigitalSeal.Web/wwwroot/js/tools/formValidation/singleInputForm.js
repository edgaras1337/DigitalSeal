import { ajaxPost } from "../utilities.js";

export default function setupSingleInputForm(inputId, onSubmit = null) {
    const input = document.getElementById(inputId);

    const inputName = input.name;
    const inputNameCamelCase = inputName.charAt(0).toLowerCase() + inputName.slice(1);

    const $input = $(input);
    const inputContainer = input.closest('.editable-input');
    const form = input.closest('form');
    const submitUrl = form.getAttribute('action');

    let initialValue = input.value;

    const saveButton = inputContainer.querySelector('.save-edit-button');
    const startEditButton = inputContainer.querySelector('.start-edit-button');
    const cancelEditButton = inputContainer.querySelector('.cancel-edit-button');

    input.addEventListener('input', () => {
        saveButton.disabled = !$input.valid();
    });

    const editMode = inputContainer.querySelector('.edit-mode');
    const defaultMode = inputContainer.querySelector('.default-mode');

    const defaultModeValue = defaultMode.querySelector('.value');

    startEditButton.addEventListener('click', () => {
        saveButton.disabled = input.value == initialValue;
        toggleModes();
    });

    cancelEditButton.addEventListener('click', () => {
        input.value = initialValue;
        $input.valid();
        toggleModes();
    });

    saveButton.addEventListener('click', () => {
        ajaxPost(submitUrl, new FormData(form), data => {
            toggleModes();
            const newValue = data[inputNameCamelCase];
            input.value = newValue;
            defaultModeValue.innerText = newValue;

            initialValue = input.value;

            onSubmit?.(data);
        });
    });

    function toggleModes() {
        editMode.classList.toggle('hidden');
        defaultMode.classList.toggle('hidden');
    }
}