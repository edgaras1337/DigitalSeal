$(() => {
    onPageChange(document.querySelector('#page-select option').value);

    document.getElementById('page-select').addEventListener('change', (e) => {
        const page = e.target.value;
        onPageChange(page);
    });

    function onPageChange(page) {
        document.querySelectorAll(`#position-select .hidden`).forEach(element => element.classList.remove('hidden'));

        document.querySelectorAll(`[data-page="${page}"]`).forEach((element) => {
            const position = element.getAttribute('data-position');
            document.querySelector(`#position-select option[value="${position}"]`).classList.add('hidden');
            document.querySelector('#position-select option:not(.hidden)').selected = true;
        });
    }

    document.querySelector('.cancel-btn').addEventListener('click', (e) => {
        e.preventDefault();
        window.location.href = document.querySelector('#ReturnUrl').value || '/';
    });

});