import docEditApi from "./apiCalls/docEditApi.js";
import docListApi from "./apiCalls/docListApi.js";
import signatureApi from "./apiCalls/signatureApi.js";
import { createGridAsync } from "./tools/agGrid.js"
import { setupGridItemRemoval } from "./tools/gridFunctionManager.js";
import { addFullHeightToBodyAndHtml } from "./tools/utilities.js";

$(async () => {
    addFullHeightToBodyAndHtml();

    //const filterPanel = setupFilterPanel(category => docGrid.refreshGridData(getDocumentListUrl(category)));

    const gridDivID = 'primary-list';
    const docGrid = await createGridAsync(getDocumentListUrl(), gridDivID);

    setupGridButtons();
    setupFileUpload();
    //setupSignaturesModal();

    function setupFileUpload() {
        const fileInput = document.getElementById('file-input');
        document.querySelector('.add-button').addEventListener("click", (e) => {
            e.preventDefault();
            fileInput.click();
        });

        fileInput.addEventListener('change', () => {
            const file = fileInput.files[0];
            docListApi.createDoc(file)
                .then(() => {
                    fileInput.value = '';

                    //const currCategory = filterPanel.getCurrentValue();
                    //if (currCategory !== 'All' && currCategory !== 'Personal') {
                    //    filterPanel.clickTab('Personal');
                    //} else {
                        docGrid.refreshGridData(getDocumentListUrl());
                    //}

                    //const currentCategory = getSelectedCategory();
                    //const currentCategory = filterPanel.getCurrentValue();
                    //if (currentCategory === 'All' || currentCategory === 'Personal') {
                    //    docGrid.refreshGridData(getDocumentListUrl());
                    //} else {
                    //    onTabClick(document.querySelector('.list-item:has(input[value="Personal"])'));
                    //}
                });
        });
    }

    function setupGridButtons() {
        setupGridItemRemoval(docGrid, docListApi.deleteDocs, 'confirm-doc-delete-modal');

        const click = (btnId, func) => document.getElementById(btnId).addEventListener('click', func);
        click('edit-button', () => docEditApi.redirectToEditPage(docGrid.getSelectedKeys()[0]));
        click('sign-button', () => signatureApi.redirectToSignPage(docGrid.getSelectedKeys()));
    }

    function getDocumentListUrl(category = null) {
        //return docListApi.getListUrl(category ?? filterPanel.getCurrentValue())
        return docListApi.getListUrl('All')
    }
});