import { createGridAsync, getGrid } from "./tools/agGrid.js"
import { setupModalServerAction } from "./tools/utilities.js";
import orgEditApi from "./apiCalls/orgEditApi.js";
import orgListApi from "./apiCalls/orgListApi.js";
import { setupModalGrid } from "./tools/gridFunctionManager.js";
import setupSingleInputForm from "./tools/formValidation/singleInputForm.js";

$(async () => {
    const orgId = document.getElementById("OrgId").value;
    const isAuthor = document.getElementById("IsOwner").value;
    const returnUrl = document.getElementById("ReturnUrl").value;

    setupSingleInputForm('Name');

    setupPartyInvite();
    setupOrgSwitch();
    if (isAuthor) {
        setupOrgDelete();
        setupUserKick();
    } else {
        setupOrgLeave();
    }

    const partyGrid = await createGridAsync(orgEditApi.getPartyListUrl(orgId), 'party-list');
    const pendingPartyGrid = await createGridAsync(orgEditApi.getPendingPartyListUrl(orgId), 'pending-party-list');

    function setupOrgSwitch() {
        const button = document.getElementById('switch-org-button');
        const disableIfCurrent = document.getElementsByClassName('disable-if-current');
        button.addEventListener('click', async () => {
            await orgEditApi.setAsCurrent(orgId);
            for (const btn of disableIfCurrent)
                btn.disabled = true;
        });
    }

    function setupOrgDelete() {
        setupModalServerAction('confirm-org-delete-modal', () => {
            return orgListApi.deleteOrgs([orgId])
                .then(() => window.location.href = returnUrl);
        });
    }

    function setupUserKick() {
        setupModalServerAction('confirm-party-remove-modal', () => {
            return orgEditApi.kickParties(orgId, partyGrid.getSelectedKeys())
                .then(() => partyGrid.refreshGridData());
        });
    }

    function setupPartyInvite() {
        const possiblePartyGridId = 'possible-party-list';
        setupModalGrid('possible-party-modal', orgEditApi.getPossiblePartyListUrl(orgId), possiblePartyGridId, inviteParties);

        function inviteParties() {
            return orgEditApi
                .inviteParties(orgId, getGrid(possiblePartyGridId).getSelectedKeys())
                .then(() => Promise.all([partyGrid.refreshGridData(), pendingPartyGrid.refreshGridData()]));
        }
    }

    function setupOrgLeave() {
        setupModalServerAction('confirm-org-leave-modal', () => {
            return orgEditApi.leaveOrg(orgId)
                .then(() => location.href = returnUrl);
        });
    }
});