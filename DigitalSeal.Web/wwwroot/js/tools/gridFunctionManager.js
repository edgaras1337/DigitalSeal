import { createGridAsync } from "./agGrid.js";
import { ajaxCall, buttonServerCall, setupModalServerAction } from "./utilities.js"

/**
 * 
 * @param {any} grid
 * @param {(rowKeys: string[]) => Promise} ajaxDeleteFunc
 * @param {string} confirmModalId
 */
export function setupGridItemRemoval(grid, ajaxDeleteFunc, confirmModalId) {
    const $deleteConfirmModal = $('#' + confirmModalId);
    const deleteBtn = $deleteConfirmModal.find('.confirm-button')[0];

    const hideModal = () => $deleteConfirmModal.modal('hide');
    const hideAndRefresh = () => {
        hideModal();
        grid.refreshGridData();
    }

    const onClick = () => {
        return ajaxDeleteFunc(grid.getSelectedKeys())
            .then(hideAndRefresh)
            .catch(hideModal);
    }

    buttonServerCall(deleteBtn, onClick);
}

export function setupModalGrid(modalId, listUrl, listId, onSubmit) {
    const modal = document.getElementById(modalId);
    let gridOptions = null;
    modal.addEventListener('show.bs.modal', async () => {
        if (!gridOptions) {
            gridOptions = await createGridAsync(listUrl, listId);
        } else {
            await gridOptions.refreshGridData();
        }
    });

    if (onSubmit) {
        setupModalServerAction(modalId, onSubmit);
    }
}