import { ajaxDelete, ajaxPost } from "../tools/utilities.js";

const initUrl = '/organizations';

const orgListApi = (() => {
    /**
     * Creates an URL for the organization list.
     * @param {string} category Category value.
     * @returns {string} URL of the organization list endpoint.
     */
    const getListUrl = (category) => {
        const params = new URLSearchParams({ category });
        return initUrl + '/list?' + params.toString();
    }

    /**
     * 
     * @param {number[]} orgIds
     */
    const deleteOrgs = (orgIds) => {
        const params = new URLSearchParams(orgIds.map(x => ['orgIds', x]));
        const url = initUrl + '?' + params.toString();
        return ajaxDelete(url);
    }

    return {
        getListUrl,
        deleteOrgs
    }
})();

export default orgListApi;