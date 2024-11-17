const button = document.getElementById("send-again-button");
button.disabled = true;

export function initSendAgainButton(onSend) {
    button.addEventListener('click', () => {
        onSend();
    });
}

export function launchCounter(msUntilEnabled = 11000) {
    const sendAgainSuggestion = document.getElementById("send-again-suggestion");
    const counter = document.getElementById('counter');

    const countDownDate = new Date(new Date().getTime() + msUntilEnabled);

    function launchCounter() {
        counter.innerText = (msUntilEnabled - 1000) / 1000;

        // Update the count down every 1 second
        const counterInterval = setInterval(() => {
            const now = new Date().getTime();
            const distance = countDownDate - now;

            var seconds = Math.floor((distance % (1000 * 60)) / 1000);
            counter.innerText = seconds;

            if (distance <= 1000) {
                clearInterval(counterInterval);
                button.disabled = false;
                sendAgainSuggestion.classList.add('hidden');
            }
        }, 1000);
    }
    launchCounter();
}