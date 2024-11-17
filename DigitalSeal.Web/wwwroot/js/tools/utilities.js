export function ajaxCall(options) {
    const defaultErrorCallback = (xhr) => {
        getResponseHeaders(xhr)
        console.error(xhr);
    };
    options.error = [defaultErrorCallback, options.error];
    const [path, params] = options.url.split('?');
    const url = new URLSearchParams(params);
    const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    url.append('TimeZone', timeZone);
    options.url = path + '?' + url.toString();

    if (options.data instanceof FormData) {
        options.processData = false;
        options.contentType = false;
    }
    return $.ajax(options);
}

export function ajaxGet(url, success, options) {
    return ajaxCall({ url, type: 'GET', success, ...options });
}

export function ajaxDelete(url, success, options) {
    return ajaxCall({ url, type: 'DELETE', success, ...options });
}

export function ajaxPost(url, data, success, error, options) {
    return ajaxCall({ url, type: 'POST', data, success, error, ...options });
}

export function ajaxPut(url, data, success, options) {
    return ajaxCall({ url, type: 'PUT', data, success, ...options });
}

export function getCurrentUrl() {
    return window.location.pathname + window.location.search;
}

export function createUrl(path, paramsObj) {
    if (!paramsObj) {
        return path;
    }

    const prefix = path.includes('?') ? '&' : '?';

    const sp = new URLSearchParams();
    for (const key in paramsObj) {
        sp.append(key, paramsObj[key]);
    }

    return path + prefix + sp.toString();
}

/**
 * 
 * @param {HTMLElement} button
 * @param {any} onClick
 */
export function buttonServerCall(button, onClick) {
    const title = button.querySelector('.title');
    const loader = button.querySelector('.loading-container');
    button.addEventListener('click', async () => {
        title.classList.add('opacity-0');
        loader.classList.remove('hidden');
        button.disabled = true;

        try {
            await onClick();
        } finally {
            loader.classList.add('hidden');
            title.classList.remove('opacity-0');
            button.disabled = false;
        }
    });
}

export function setupModalServerAction(modalId, action) {
    const modalSelector = `#${modalId}`;
    const confirmBtn = document.querySelector(`${modalSelector} .confirm-button`);
    buttonServerCall(confirmBtn, async () => {
        try {
            await action();
        } finally {
            $(modalSelector).modal('hide');
        }
    })
}

export function addFullHeightToBodyAndHtml() {
    document.documentElement.style.height = '100%';
    document.body.style.height = '100%';
}