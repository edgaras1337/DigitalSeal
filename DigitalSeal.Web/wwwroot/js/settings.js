import { trackFormState } from "./tools/formValidation/trackFormState.js"
import { handleFormSubmit } from "./tools/formValidation/handleFormSubmit.js";

$(async () => {
    "use strict";

    handlePasswordForm();
    handleDetailsForm();

    function handleDetailsForm() {
        const updateProfileForm = document.forms['update-profile'];
        trackFormState(updateProfileForm, (data) => {
            if (data.emailUpdated && data.emailConfirmationUrl) {
                location.href = data.emailConfirmationUrl;
            }
        });
    }

    function handlePasswordForm() {
        const updatePasswordForm = document.forms['update-password'];

        const passwordInputs = updatePasswordForm.querySelectorAll('.form-input');
        const saveBtn = updatePasswordForm.querySelector('.form-submit');
        passwordInputs.forEach(input => {
            input.addEventListener('input', () => {
                saveBtn.disabled = hasInvalidInputs(passwordInputs);
            });
        })

        function hasInvalidInputs(inputs) {

            let isInvalid = false;
            for (const input of inputs) {
                // This is a workaround to trigger validation on "input" event.
                // jQuery seems to only add the validation on "input" after the input had a change or blur event.
                if (input.name === 'ConfirmPassword' && input.value) {
                    $(input).valid();
                }

                const isError = input.classList.contains('input-validation-error');
                if (!input.value || isError) {
                    isInvalid = true;
                    break;
                }
            }

            return isInvalid;
        }

        handleFormSubmit(updatePasswordForm, () => {
            updatePasswordForm.reset();
        });
    }


    //const usersList = document.getElementById('users-list');
    //if (usersList) {
    //    const userGridOptions = await createGridAsync('/settings/users', 'users-list', refreshButtonState);

    //    document.getElementById('delete-confirm-btn').addEventListener('click', () => {
    //        const userIDs = userGridOptions.api.getSelectedRows()?.map(row => row.id);
    //        if (!userIDs) return;

    //        const params = new URLSearchParams();
    //        userIDs.forEach((userID) => params.append("userIDs", userID));

    //        const success = async () => {
    //            await userGridOptions.aggFuncs.refreshGridData();
    //            refreshButtonState(userGridOptions.api.getSelectedRows());
    //        };
    //        ajaxDelete(`/settings/users?${params.toString()}`, success);
    //    });

    //    function refreshButtonState(selected) {
    //        const needSelectionButtons = document.querySelectorAll('.need-selection');
    //        needSelectionButtons.forEach(button => {
    //            button.disabled = selected.length === 0;
    //        });
    //    }
    //}

    //document.getElementById('page-tab').addEventListener('shown.bs.tab', function (e) {
    //    const activeTab = e.target.getAttribute('data-index');
    //    document.getElementById('OpenTabIndex').value = activeTab;
    //});
});
