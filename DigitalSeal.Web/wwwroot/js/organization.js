import { createGridAsync } from "./tools/agGrid.js";
import { ajaxDelete, ajaxGet, ajaxPost } from "./tools/utilities.js";

$(async () => {
    const openOrgClass = 'active-item';
    const userSelectedOrgClass = 'user-selected-org';

    const orgToOpen = document.getElementById('OrgToOpen').value || 0;
    const selectOrgButton = document.getElementById('select-org-button');

    const getActiveTab = () => document.querySelector(`.org-item.${openOrgClass}`);
    const getOrgID = () => getActiveTab().querySelector('#OrgID').value;

    let gridsRefresh;
    const titleInput = document.getElementById('title-input');

    await fillOrganizationsAsync();

    const usersUrl = () => '/organization/users/' + getOrgID();
    const possibleUsersUrl = () => '/organization/users/possible/' + getOrgID();
    const invitedUsersUrl = () => '/organization/users/invited/' + getOrgID();

    const userGridOptions = await createGridAsync(usersUrl(), 'org-users-list');
    const possibleUserGridsOptions = await createGridAsync(possibleUsersUrl(), 'possible-users-list');
    const invitedUsersGridOptions = await createGridAsync(invitedUsersUrl(), 'invited-users-list');

    gridsRefresh = () => Promise.all([
        userGridOptions.aggFuncs.refreshGridData(usersUrl()),
        possibleUserGridsOptions.aggFuncs.refreshGridData(possibleUsersUrl()),
        invitedUsersGridOptions.aggFuncs.refreshGridData(invitedUsersUrl()),
    ]);

    let initialTitle = titleInput.value;
    titleInput.addEventListener('change', () => {
        const newVal = titleInput.value;
        if (newVal !== initialTitle) {
            const orgID = getOrgID();
            ajaxPost('/organization/update', { orgID, newName: newVal }, () => {
                document.querySelector(`#OrgID[value="${orgID}"]`).closest('.list-group-item').querySelector('.title').innerHTML = newVal;
                updateNavbarOrgLink();
                initialTitle = newVal;
            });
        }
    });

    document.getElementById('create-organization-form').addEventListener('submit', (e) => {
        e.preventDefault();
        ajaxPost('/organization/add', $(e.target).serialize(), () => {
            $('#create-org-modal').modal('hide');
            fillOrganizationsAsync();
            e.target.reset();
        });
    });

    document.getElementById('delete-org-button').addEventListener('click', () =>
        ajaxDelete('/organization/delete/' + getOrgID(), async () => await fillOrganizationsAsync()));

    document.getElementById('leave-org-button').addEventListener('click', () =>
        ajaxDelete('/organization/leave/' + getOrgID(), async () => await fillOrganizationsAsync()));

    document.querySelector('.org-list').addEventListener('click', (e) => {
        const orgItem = e.target.closest('.org-item');
        if (orgItem) {
            onTabClick(orgItem);
        }
    });

    function onTabClick(tabElem) {
        document.querySelectorAll(`.org-item.${openOrgClass}`).forEach(tab => tab.classList.remove(openOrgClass));
        tabElem.classList.add(openOrgClass);

        const isCurrentOrg = tabElem.classList.contains(userSelectedOrgClass);
        document.getElementById('init-delete-org').disabled = isCurrentOrg;
        document.getElementById('init-leave-org').disabled = isCurrentOrg;

        const orgID = tabElem.querySelector('#OrgID').value;
        ajaxGet('/organization/info/' + orgID, async (data) => {
            selectOrgButton.disabled = data.isSelected;
            document.getElementById('org-id').innerHTML = data.id;
            titleInput.value = data.name;
            titleInput.disabled = !data.isOwner;
            document.getElementById('org-owner').innerHTML = data.owner;
            const ownerButton = document.querySelectorAll('.owner-button');
            const memberButton = document.querySelectorAll('.member-button');
            if (!data.isOwner) {
                ownerButton.forEach(btn => btn.classList.add('hidden'));
                memberButton.forEach(btn => btn.classList.remove('hidden'));
            } else {
                ownerButton.forEach(btn => btn.classList.remove('hidden'));
                memberButton.forEach(btn => btn.classList.add('hidden'));
            }
            await (gridsRefresh && gridsRefresh());
        });
    }

    function updateNavbarOrgLink() {
        ajaxGet('/organization/current', (name) => {
            document.getElementById('my-org-link').innerHTML = `Organization: ${name}`;
        });
    }

    async function selectOrganization(orgID = null) {
        orgID = orgID || getOrgID();
        return ajaxPost('/organization/select', { orgID }, () => {
            updateNavbarOrgLink();
        });
    }

    selectOrgButton.addEventListener('click', onSelectOrgButtonClick);

    async function onSelectOrgButtonClick() {
        await selectOrganization();
        await fillOrganizationsAsync();
    }

    document.getElementById('invite-users-button').addEventListener('click', async () => {
        const userIDs = possibleUserGridsOptions.api.getSelectedRows().map(row => row.userId);
        ajaxPost('/organization/invite', { orgID: getOrgID(), userIDs }, async () => {
            await gridsRefresh();
        });
    });

    document.getElementById('remove-users-button').addEventListener('click', async () => {
        const params = new URLSearchParams();
        params.append('orgID', getOrgID());

        userGridOptions.api.getSelectedRows()
            .forEach(row => params.append('userIDs', row.userId));

        ajaxDelete('/organization/users/remove?' + params.toString(), async () => {
            await gridsRefresh();
        });
    });

    async function fillOrganizationsAsync() {
        await ajaxGet('/organization/all', (data) => generateTabs(data));
    }

    function generateTabs(data) {
        let lastTab = null;
        let isTabClicked = false;
        let selectedTab = null;
        if (data) {
            document.querySelectorAll('.org-item:not(.template)').forEach(tab => tab.remove());
            data.forEach(org => {
                const template = document.querySelector('.org-item.template');
                if (!lastTab)
                    lastTab = template;

                const tab = template.cloneNode(true);
                tab.classList.remove('hidden', 'template');
                tab.querySelector('.title').innerHTML = org.name;
                tab.querySelector('.org-owner').innerHTML = org.owner;
                tab.querySelector('#OrgID').value = org.id;

                if (org.isSelected) {
                    tab.classList.add(userSelectedOrgClass);
                    tab.querySelector('.icon-container').classList.remove('hidden');
                    selectedTab = tab;
                }
                else
                    selectOrgButton.disabled = false;

                if ((org.id == orgToOpen || (orgToOpen == 0 && org.isSelected)) && !isTabClicked) {
                    onTabClick(tab);
                    isTabClicked = true;
                }

                lastTab.after(tab);
                lastTab = tab;
            });

            if (!isTabClicked) {
                onTabClick(selectedTab);
            }
        }
    }
});