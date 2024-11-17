import orgEditApi from "./apiCalls/orgEditApi.js";
import orgListApi from "./apiCalls/orgListApi.js";
import { createGridAsync } from "./tools/agGrid.js"
import { trackFormState } from "./tools/formValidation/trackFormState.js";
import { setupGridItemRemoval } from "./tools/gridFunctionManager.js";
import { setupFilterPanel } from "./tools/listFilterPanel.js";
import { addFullHeightToBodyAndHtml } from "./tools/utilities.js";

$(async () => {
    addFullHeightToBodyAndHtml();

    //const filterPanel = setupFilterPanel(category => orgGrid.refreshGridData(getOrgListUrl(category)));

    const gridDivId = 'primary-list';
    const orgGrid = await createGridAsync(orgListApi.getListUrl('All'), gridDivId);
    setupGridButtons();

    //trackFormState(document.forms['create-org'], (data) => orgEditApi.redirectToEditPage(data.orgId));

    //formStateTracker(document.forms['create-org'], null, orgEditApi.getCreateOrgUrl(),
    //    (data) => orgEditApi.redirectToEditPage(data.orgId));

    trackFormState(document.forms['create-org']);

    function setupGridButtons() {
        setupGridItemRemoval(orgGrid, orgListApi.deleteOrgs, 'confirm-org-delete-modal');

        const click = (btnId, func) => document.getElementById(btnId).addEventListener('click', func);
        click('edit-button', () => orgEditApi.redirectToEditPage(orgGrid.getSelectedKeys()[0]));
        click('set-button', () => {
            orgEditApi.setAsCurrent(orgGrid.getSelectedKeys()[0])
                .then(() => orgGrid.refreshGridData())
        });
    }
});