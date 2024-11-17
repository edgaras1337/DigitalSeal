import { initSendAgainButton, launchCounter } from "./tools/sendAgainButton.js"

$(() => {
    initSendAgainButton(() => location.reload());
    launchCounter();
})