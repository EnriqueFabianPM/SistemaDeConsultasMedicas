window.generateMap = function () {
    window.appMap = new google.maps.Map(document.getElementById("map"), {
        center: { lat: 19.4326, lng: -99.1332 },
        zoom: 12
    });
    window.appMarker = null;
};

window.updateMapMarkers = function (lat, lng, title) {
    if (!window.appMap) return;

    const position = { lat: parseFloat(lat), lng: parseFloat(lng) };

    if (window.appMarker) {
        window.appMarker.setPosition(position);
    } else {
        window.appMarker = new google.maps.Marker({
            position,
            map: window.appMap,
            title: title
        });
    }

    window.appMap.setCenter(position);
};