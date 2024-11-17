import { ajaxDelete, ajaxPost } from "../tools/utilities.js";

const initUrl = '/organization';

const orgEditApi = (() => {
    ///**
    // * 
    // * @param {number} orgID
    // * @returns
    // */
    //const setAsCurrent = (orgID) => {
    //    const params = new URLSearchParams({ orgID });
    //    const url = initUrl + '/set?' + params.toString();
    //    return ajaxPost(url)
    //}

    /**
     * 
     * @param {number} orgId
     * @param {string} returnUrl
     */
    const redirectToEditPage = (orgId) => {
        const returnUrl = window.location.pathname;
        const params = new URLSearchParams({ orgId, returnUrl });
        window.location.href = initUrl + '?' + params.toString();
    }

    const getCreateOrgUrl = () => initUrl + '/create'

    const setAsCurrent = (orgId) => ajaxPost(initUrl + '/set-current', { orgId });

    const getPartyListUrl = (orgId) => `${initUrl}/parties/${orgId}`;

    const getPossiblePartyListUrl = (orgId) => `${initUrl}/parties/possible/${orgId}`;

    const getPendingPartyListUrl = (orgId) => `${initUrl}/parties/pending/${orgId}`;

    const inviteParties = (orgId, userIds) => ajaxPost(`${initUrl}/parties/invite`, { orgId, userIds });


    const kickParties = (orgId, partyIds) => {
        const query = new URLSearchParams({ orgId, partyIds }).toString();
        return ajaxDelete(`${initUrl}/parties/kick?${query}`);
    }

    const leaveOrg = (orgId) => {
        const query = new URLSearchParams({ orgId }).toString();
        return ajaxDelete(`${initUrl}/leave?${query}`);
    }

    return {
        redirectToEditPage,
        getCreateOrgUrl,
        setAsCurrent,
        getPartyListUrl,
        getPossiblePartyListUrl,
        inviteParties,
        getPendingPartyListUrl,
        kickParties,
        leaveOrg,
    }
})();

export default orgEditApi;

