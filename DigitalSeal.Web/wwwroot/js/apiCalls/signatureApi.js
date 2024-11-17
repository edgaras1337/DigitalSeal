const initUrl = '/signing'

const signatureApi = (() => {
    /**
     * 
     * @param {number[]} docIDs
     */
    const redirectToSignPage = (docIDs) => {
        const params = new URLSearchParams(docIDs.map(x => ['docIds', x]));
        params.append('ReturnUrl', location.pathname + location.search);
        window.location.href = initUrl + '?' + params.toString();
    }

    return {
        redirectToSignPage
    }
})();

export default signatureApi;