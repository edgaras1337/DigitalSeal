import { ajaxGet } from "./tools/utilities.js";

$(() => {
    // Set notification count
    ajaxGet('/account/notification-count', (count) => {
        document.querySelectorAll('.navbar .notification-bubble').forEach((element) => {
            if (count > 0) {
                element.textContent = count;
                element.classList.remove('hidden');
            } else {
                element.classList.add('hidden');
                element.textContent = '';
            }
        });
    });

    if (document.querySelector('.validate-timezone')) {
        const currentTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        console.log(currentTimeZone);
        const url = `/account/validate-timezone?clientTimezone=${currentTimeZone}`;

        ajaxGet(url, updateTimezones => {

            if (updateTimezones) {
                // update timezone values.

                location.reload();
            }
        });
    }


    $.validator.methods.range = function (value, element, param) {
        if ($(element).attr('data-val-date')) {
            var min = $(element).attr('data-val-range-min');
            var max = $(element).attr('data-val-range-max');
            var date = new Date(value).getTime();
            var minDate = new Date(min).getTime();
            var maxDate = new Date(max).getTime();
            return this.optional(element) || (date >= minDate && date <= maxDate);
        }
        // use the default method
        return this.optional(element) || (value >= param[0] && value <= param[1]);
    };
});