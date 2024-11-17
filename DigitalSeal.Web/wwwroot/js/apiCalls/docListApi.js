import { ajaxDelete, ajaxPost } from "../tools/utilities.js";

const initUrl = '/documents';

const docListApi = (() => {
    /**
     * 
     * @param {File} fileData
     * @returns
     */
    const createDoc = (fileData) => {
        const formData = new FormData();
        formData.append('File', fileData);
        return ajaxPost(initUrl, formData, null, { processData: false, contentType: false });
    }

    /**
     * 
     * @param {string} category
     * @returns
     */
    const getListUrl = (category) => {
        const params = new URLSearchParams({ category });
        return initUrl + '?' + params.toString();
    }

    /**
     * 
     * @param {number[]} docIDs
     */
    const deleteDocs = (docIDs) => {
        const params = new URLSearchParams(docIDs.map(x => ['docIDs', x]));
        const url = initUrl + '?' + params.toString();
        return ajaxDelete(url);
    }

    return {
        createDoc,
        getListUrl,
        deleteDocs,
    }
})();

export default docListApi;