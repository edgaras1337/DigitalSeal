import { ajaxPost } from "./tools/utilities.js";

$(() => {
    ajaxPost('/account/see-notifications', null, () => {
        const notifBubbles = document.querySelectorAll('.navbar .notification-bubble');
        notifBubbles.forEach(bubble => bubble.classList.add('hidden'));
    });
});