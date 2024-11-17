"use strict";

import { ajaxGet, ajaxPost, createUrl } from "./utilities.js";

const pageGrids = [];

export function getGrid(gridId) {
    return pageGrids.find(x => x.gridId == gridId);
}

/**
 * 
 * @param {string} dataUrl
 * @param {string} gridId
 * @param {() => void} onRowSelected
 * @returns
 */
export async function createGridAsync(dataUrl, gridId, onRowSelected) {
    // get div to host the grid
    const gridDiv = document.getElementById(gridId);
    const initialUrl = dataUrl;

    //const gridRowStyles = Object.freeze({
    //    Default: 'grid-row-default',
    //    Positive: 'grid-row-positive',
    //    Negative: 'grid-row-negative',
    //    Primary: 'grid-row-primary',
    //    Warning: 'grid-row-warning',
    //});

    const gridRowStyles = Object.freeze({
        default: 'Default',
        positive: 'Positive',
        negative: 'Negative',
        primary: 'Primary',
        warning: 'Warning',
    });

    const selectionMode = Object.freeze({
        none: 0,
        single: 1,
        multi: 2
    });

    const columnData = getColumnData();
    const keyCode = columnData.columns.find(col => col.isKey).code;



    const gridOptions = {
        columnDefs: getColumnDefs(),

        //isRowSelectable: (params) => {
        //    console.log("a");
        //    return true;
        //    //const item = records.find(rec => rec.rowData.)
        //    //const dict = gridOptions.aggFuncs.columnData.rowDisabledStatusDictionary;
        //    //if (dict) {
        //    //    for (let key in dict) {
        //    //        if (params.data[key] === dict[key])
        //    //            return false;
        //    //    }
        //    //}

        //    //return true;
        //},

        getRowId: params => params.data[keyCode],

        //getRowClass: params => {
        //    const record = getCurrentRecord(params.node, records);
        //    return gridRowStyles[record.style];
        //},

        rowClassRules: {
            'grid-row-default': (params) => checkStyle(params, gridRowStyles.default),
            'grid-row-positive': (params) => checkStyle(params, gridRowStyles.positive),
            'grid-row-negative': (params) => checkStyle(params, gridRowStyles.negative),
            'grid-row-primary': (params) => checkStyle(params, gridRowStyles.primary),
            'grid-row-warning': (params) => checkStyle(params, gridRowStyles.warning),
        },

        // default col def properties get applied to all columns
        defaultColDef: {
            sortable: true,
            filter: true,
            resizable: true,
            showDisabledCheckboxes: true,
        },

        rowSelection: columnData.selectionMode == selectionMode.multi ? 'multiple' : 'single', // allow rows to be selected
        animateRows: true, // have rows animate to new positions when sorted

        onSelectionChanged: onSelectionChanged,


        pagination: true,
        onPaginationChanged: () => {
            //console.log("test page");
        },
        paginationPageSize: 20,


        // This was an attempt to turn on infinite scrolling.
        // This can be done, but then sorting and filtering won't work - it is available only on the enterprise version of the grid.
        // Therefore, a workaround was created for sorting, which worked, but AG Grid has a bug, that when normal sorting is triggered,
        // on the infinite row mode, it sends out two duplicate data requests to the server.

        //rowBuffer: 0,
        //rowSelection: 'multiple',
        //// tell grid we want virtual row model type
        //rowModelType: 'infinite',
        //// how big each page in our page cache will be, default is 100
        //cacheBlockSize: 20,//100,
        //// how many extra blank rows to display to the user at the end of the dataset,
        //// which sets the vertical scroll and then allows the grid to request viewing more rows of data.
        //// default is 1, ie show 1 row.
        //cacheOverflowSize: 2,
        //// how many server side requests to send at a time. if user is scrolling lots, then the requests
        //// are throttled down
        //maxConcurrentDatasourceRequests: 1,
        //// how many rows to initially show in the grid. having 1 shows a blank row, so it looks like
        //// the grid is loading from the users perspective (as we have a spinner in the first col)
        //infiniteInitialRowCount: 20,
        //// how many pages to store in cache. default is undefined, which allows an infinite sized cache,
        //// pages are never purged. this should be set for large data to stop your browser from getting
        //// full of data
        ////maxBlocksInCache: 1,//10,

        //onSortChanged: (params) => {
        //    const column = params.columns[params.columns.length - 1];
        //    console.log(column);


        //    const dataSource = {
        //        rowCount: undefined, // behave as infinite scroll

        //        getRows: (params) => {

        //            const dataOptions = {
        //                start: params.startRow,
        //                end: params.endRow,
        //                sort: {
        //                    type: column.sort,
        //                    field: column.colId,
        //                }
        //            }

        //            dataUrl = createUrl(initialUrl);
        //            ajaxPost(dataUrl, dataOptions, (data) => {
        //                const rows = data.dataRows;
        //                //initOrUpdateColumnDefs(data, gridApi);

        //                let lastRow = -1;
        //                if (data.length <= params.endRow) {
        //                    lastRow = data.length;
        //                }
        //                params.successCallback(rows, lastRow);
        //            });
        //        },
        //    };
        //    gridApi.setGridOption('datasource', dataSource);
        //},

        //rowData: {},

        //aggFuncs: {
        //    /** @type {(newDataUrl: string | null) => Promise} */
        //    refreshGridData: refreshGridData,

        //    columnData: columnData
        //}
    };

    function getCurrentRecord(agGridParams, records) {
        return records.find(rec => rec.fields[keyCode] == agGridParams.id);
    }

    function checkStyle(agGridParams, style) {
        return getCurrentRecord(agGridParams.node, records).style === style;
    }

    function getColumnData() {
        const columnData = JSON.parse(gridDiv.getAttribute("data-column-defs"));
        return columnData;
    }

    function getColumnDefs() {
        const options = getColumnData();
        
        const defs = [];
        let selectionEnabled = false;
        options.columns.map((col, i) => {
            const colDef = { field: col.code, headerName: col.title };
            if (options.selectionMode != selectionMode.none) {
                const allowSelection = !selectionEnabled && !col.isHidden;
                if (allowSelection)
                    selectionEnabled = true;
                colDef.headerCheckboxSelection = options.selectionMode === selectionMode.multi && allowSelection;
                colDef.checkboxSelection = allowSelection;
            }
            colDef.hide = col.isHidden;
            defs.push(colDef);
        });
        return defs;
    }

    /**
     * Repopulates grid with new data. Data can be fetched from a new URL, or the initial one.
     * @param {string | null} newDataUrl
     */
    function refreshGridData(newDataUrl) {
        //newDataUrl ??= dataUrl;
        //return ajaxGet(newDataUrl ?? dataUrl, (data) => {
        //    initOrUpdateColumnDefs(data);
        //    gridOptions.api.updateGridOptions({ rowData: data.dataRows });
        //    disableSpecificRows(data);
        //});

        gridApi.showLoadingOverlay();

        return ajaxGet(newDataUrl ?? dataUrl, data => {
            records = data.records;
            gridApi.updateGridOptions({ rowData: data.records.map(rec => rec.fields) });
            disableSpecificRows(records);
            gridApi.hideOverlay();
        });
    }

    const listContainer = document.getElementById(`${gridId}-container`);
    const needSelectionElements = listContainer?.querySelectorAll('.need-selection');
    const singleSelectionElements = listContainer?.querySelectorAll('.need-selection.single');
    if (needSelectionElements) {
        for (const elm of needSelectionElements)
            elm.disabled = true;
    }

    if (singleSelectionElements) {
        for (const elm of singleSelectionElements)
            elm.disabled = true;
    }

    function onSelectionChanged() {
        if (!listContainer)
            return;
        
        const selectedRows = gridApi.getSelectedRows();

        needSelectionElements?.forEach((button) => button.disabled = selectedRows.length === 0);
        singleSelectionElements?.forEach((button) => button.disabled = selectedRows.length === 0 || selectedRows.length > 1);

        if (onRowSelected) {
            onRowSelected(selectedRows);
        }
    }

    //if (!dataUrl || !gridDiv)
    //    return null;

    //const data = await ajaxGet(dataUrl);
    //gridOptions.rowData = data.dataRows ?? {};

    //function initOrUpdateColumnDefs(data, gridApi) {
    //    const defs = [];
    //    data.columns.map((col, i) => {
    //        const colDef = { field: col };
    //        const allowSelection = i == 0 && !data.disableAllselection;
    //        colDef.checkboxSelection = allowSelection;
    //        defs.push(colDef);
    //    });
    //    gridApi.updateGridOptions({ columnDefs: defs });
    //}

    function disableSpecificRows(records) {
        gridApi.setGridOption('isRowSelectable', params => {
            const record = getCurrentRecord(params, records)
            return record?.isSelectable;
        });
    }

    //initOrUpdateColumnDefs(data);


    //new agGrid.Grid(gridDiv, gridOptions);
    const gridApi = agGrid.createGrid(gridDiv, gridOptions);

    let { records } = await ajaxGet(dataUrl);

    disableSpecificRows(records);
    gridApi.setGridOption('rowData', records.map(rec => rec.fields) ?? {});


    function getSelectedKeys() {
        const rows = gridApi.getSelectedRows();
        return rows.map(row => row[keyCode]);
    }


    const gridWrapper = {
        api: gridApi,
        rowIdCode: keyCode,
        refreshGridData: refreshGridData,
        columnData: columnData,
        getSelectedKeys
    }

    pageGrids.push({ gridId, ...gridWrapper });
    window.addEventListener('beforeunload', () => {
        let i = pageGrids.length;
        while (i--) {
            const api = pageGrids[i]?.gridWrapper?.api;
            api?.destroy();
            pageGrids.splice(i, 1);
        }
    });

    return gridWrapper;
}