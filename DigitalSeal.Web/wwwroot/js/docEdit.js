import { createGridAsync, getGrid } from "./tools/agGrid.js"
import { setupModalServerAction } from "./tools/utilities.js";
import { signatureStatus } from "./tools/signatureStatus.js"
import docEditApi from "./apiCalls/docEditApi.js";
import { setupModalGrid } from "./tools/gridFunctionManager.js";
import signatureApi from "./apiCalls/signatureApi.js";
import setupSingleInputForm from "./tools/formValidation/singleInputForm.js";

$(async () => {
    const docIdInput = document.getElementById("DocId");
    const docId = docIdInput.value;
    const isAuthor = document.getElementById("IsAuthor").value;
    const returnUrl = document.getElementById("ReturnUrl").value;

    setupSingleInputForm('Name');
    setupSingleInputForm('Deadline', (data) => {
        partyGridOptions.refreshGridData()
            .catch(err => console.error(err));

        updateSigningStatus(data.status);
        const deletePartyButton = document.querySelector('.init-delete-party');
        if (!data.isDeadlinePassed) {
            const addPartyButton = document.querySelector('.init-add-party');
            const deadlineWarning = document.getElementById('deadline-warning');

            addPartyButton.disabled = false;
            deletePartyButton.classList.add('need-selection');
            deadlineWarning.classList.add('hidden');
        } else {
            deletePartyButton.classList.remove('need-selection');
        }
    });

    const partyGridOptions = await createGridAsync('/documents/edit/parties/' + docId, 'party-list');
    setupModalGrid('signatures-modal', docEditApi.getSignaturesListUrl(docId), 'signatures-list');
    await setupAuthorFunctions();

    document.getElementById('sign-button')?.addEventListener('click',
        () => signatureApi.redirectToSignPage([docId]));

    async function setupAuthorFunctions() {
        if (!isAuthor)
            return;

        const possiblePartyGridId = 'possible-party-list';
        setupModalGrid('possible-party-modal', docEditApi.getPossibDocPartyListUrl(docId),
            possiblePartyGridId, addDocParties);

        setupModalServerAction('confirm-doc-delete-modal', deleteDoc);
        setupModalServerAction('confirm-party-delete-modal', deleteParties);

        function deleteDoc() {
            return docEditApi.deleteDoc(docId)
                .then(() => window.location.href = returnUrl);
        }

        function addDocParties() {
            return docEditApi
                .addParties(docId, getGrid(possiblePartyGridId).getSelectedKeys())
                .then(() => afterPartyGridUpdate());
        }

        function deleteParties() {
            return docEditApi.deleteParties(docId, partyGridOptions.getSelectedKeys())
                .then(() => afterPartyGridUpdate());
        }

        function afterPartyGridUpdate() {
            docEditApi.getSignStatus(docId)
                .then(status => updateSigningStatus(status));

            return partyGridOptions.refreshGridData();
        }
    }

    function updateSigningStatus(status) {
        const alertMapping = {
            InProgress: 'primary',
            SignedLate: 'warning',
            Signed: 'success',
            NotSigned: 'danger',
        };

        const alerts = document.querySelectorAll('#sign-status-alerts .alert');
        alerts.forEach(alert => alert.classList.add('hidden'));

        if (status !== signatureStatus.none) {
            const alertClass = `.alert-${alertMapping[status]}`;
            const matchingAlert = document.querySelector(alertClass);
            if (matchingAlert) {
                matchingAlert.classList.remove('hidden');
            }
        }
    }
});