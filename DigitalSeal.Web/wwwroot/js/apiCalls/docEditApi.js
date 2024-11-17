import { ajaxDelete, ajaxGet, ajaxPost } from "../tools/utilities.js";

const initUrl = '/documents/edit';

const docEditApi = (() => {
    /**
     * 
     * @param {number} docID
     * @returns
     */
    const redirectToEditPage = (docID) => {
        const params = new URLSearchParams({ docID });
        window.location.href = initUrl + '?' + params.toString();
    }

    /**
     * Gets the signing status of a document.
     * @param {number} docID
     * @returns Promise of the call to the database.
     */
    const getSignStatus = (docID) => ajaxGet(initUrl + '/sign-status/' + docID);

    const partyUrl = initUrl + '/parties';

    /**
     * 
     * @param {number} docId
     * @param {number[]} partyIds
     * @returns
     */
    const addParties = (docId, partyIds) => ajaxPost(partyUrl + '/add', { docId, partyIds });

    /**
     * 
     * @param {number} docId
     * @param {number[]} partyIds
     * @returns
     */
    const deleteParties = (docId, partyIds) => {
        const query = new URLSearchParams({ docId, partyIds }).toString();
        return ajaxDelete(`${partyUrl}/delete?${query}`);
    }

    /**
     * 
     * @param {number} docId
     * @returns
     */
    const deleteDoc = (docId) => ajaxDelete(`${initUrl}/delete/${docId}`);

    const getDocPartyListUrl = (docId) => `${initUrl}/parties/${docId}`;

    const getPossibDocPartyListUrl = (docId) => `${initUrl}/parties/possible/${docId}`;

    const getSignaturesListUrl = (docId) => `${initUrl}/signatures/${docId}`;

    /**
     * @param {any} updateRequest
     */
    const updateDoc = (updateRequest) => {
        const url = `${initUrl}/update`;
        return ajaxPost(url, updateRequest);
    }

    return {
        redirectToEditPage,
        updateDoc,
        getSignStatus,
        addParties,
        deleteParties,
        deleteDoc,
        getDocPartyListUrl,
        getPossibDocPartyListUrl,
        getSignaturesListUrl
    }
})();

export default docEditApi;