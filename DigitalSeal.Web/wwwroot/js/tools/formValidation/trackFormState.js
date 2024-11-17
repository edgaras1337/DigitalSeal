import { ajaxPost } from "../utilities.js";
import { createFormValues } from "./handleFormSubmit.js";

export function trackFormState(form, onSubmit = null) {
    const objectId = form.querySelector('.object-id');
    const inputs = form.querySelectorAll('.form-input');
    const submitButton = form.querySelector('.form-submit');
    const submitUrl = form.getAttribute('action');

    submitButton.disabled = true;

    //form.addEventListener('submit', (e) => e.preventDefault());

    const initialValues = {};
    setInitialValues();

    inputs.forEach(input => {
        if (isCheckbox(input)) {
            input.addEventListener('change', onChange);
        } else {
            input.addEventListener('input', onChange);
        }
    });

    function onChange(e) {
        let enableSubmit = false;
        for (const input of inputs) {
            if (!isValid(input)) {
                enableSubmit = false;
                break;
            }

            if (!enableSubmit) {
                enableSubmit = hasChanges(input);
            }
        }

        toggleSubmit(enableSubmit);
    }

    submitButton.addEventListener('click', () => {
        ajaxPost(submitUrl, createFormValues(objectId, inputs), (data) => {

            inputs.forEach(input => {
                const key = input.name.charAt(0).toLowerCase() + input.name.slice(1);
                if (data.hasOwnProperty(key)) {
                    input.value = data[key];
                }
            });

            setInitialValues();
            submitButton.disabled = true;

            onSubmit?.(data);
        });
    })


    function toggleSubmit(enable) {
        submitButton.disabled = !enable;
    }

    function isValid(input) {
        return $(input).valid();
    }

    function hasChanges(input) {
        return getValue(input) != initialValues[input.name];
    }

    function isCheckbox(input) {
        return input.type === 'checkbox';
    }

    function getValue(input) {
        return isCheckbox(input) ? input.checked : input.value;
    }

    function setInitialValues() {
        inputs.forEach(input => {
            initialValues[input.name] = getValue(input);
        });
    }
}