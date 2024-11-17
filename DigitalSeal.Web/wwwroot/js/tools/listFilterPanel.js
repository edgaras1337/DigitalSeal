/**
 * 
 * @param {(filterValue: string) => any} onChange
 */
export function setupFilterPanel(onChange) {
    const activeItemClass = 'active-item';

    let defaultValue;

    const filterValues = [];

    const filterItems = [...document.querySelectorAll('.list-sidebar .filter-item')];
    filterItems.forEach(item => {
        defaultValue ??= getCurrentValueInternal(item);
        item.addEventListener('click', () => onFilterValueChange(item));

        filterValues.push(getCurrentValueInternal(item));
    });

    function onFilterValueChange(selectedFilterItem) {
        const lastButton = document.querySelector('.primary-button');
        lastButton.classList.remove('primary-button');
        lastButton.classList.add('secondary-button');
        //lastButton.querySelector('.icon-container').classList.add('hidden');

        selectedFilterItem.querySelector('.filter-button').classList.remove('secondary-button');
        selectedFilterItem.querySelector('.filter-button').classList.add('primary-button');
        //filterItem.querySelector('.icon-container').classList.remove('hidden');

        onChange(getCurrentValueInternal(selectedFilterItem));


        //const lastItem = document.querySelector(`.filter-item.${activeItemClass} .filter-button`);
        //lastItem.classList.remove(activeItemClass);
        //lastItem.querySelector('.icon-container').classList.add('hidden');

        //filterItem.classList.add(activeItemClass);
        //filterItem.querySelector('.icon-container').classList.remove('hidden');

        //onChange(getCurrentValueInternal(filterItem));
    }

    function getCurrentValueInternal(filterItem) {
        return filterItem ? filterItem.querySelector('.category-value').value : getCurrentValue()
    }

    function getCurrentValue() {
        const activeItem = document.querySelector('.list-sidebar .active-item');
        return activeItem.querySelector('.category-value')?.value;
    }

    function clickTab(value) {
        if (!filterValues.includes(value)) {
            console.error('No such filter value found');
            return;
        }

        const filterItem = filterItems.find(item => getCurrentValueInternal(item) === value);
        if (filterItem) {
            onFilterValueChange(filterItem);
        }
    }

    return {
        /**
         * Gets the current filter panel value.
         * @type {() => string}
         */
        getCurrentValue,

        /**
         * Default filter panel value. (The first one).
         * @type {string}
         */
        defaultValue,

        clickTab
    }
}

