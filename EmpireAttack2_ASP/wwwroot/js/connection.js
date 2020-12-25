/*
* 
* Connection Methods
* 
* */
//Connection class to /game endpoint
var connection;

function buildConnection() {
    connection = new signalR.HubConnectionBuilder().withUrl("/game").build();

    //What to do on connction start
    connection.start().then(function () {
        //Fetch Available Factions
        connection.invoke("GetFactions").catch(function (err) {
            return console.error(err.toString());
        });
    }).catch(function (err) {
        return console.error(err.toString());
    });

    connection.on("ReceiveFactions", function (factions) {
        var facs = factions.split(':');
        console.log(facs);
        facs.forEach(function (element) {
            createButton(element);
        });
    });

    connection.on("ReceiveBeat", function () {
        console.log("Beat");
    });

    connection.on("LoginAnswer", function (success) {
        if (!success) {
            alert("Login failed. Please try again...");
            return;
        }
        connection.invoke("SendMap").catch(function (err) {
            return console.error(err.toString());
        });
    });

    connection.on("DownloadMap", function (mapData) {
        mapDataToTileMap(mapData);
        loadGame();
    });

    connection.on("Cl_FastTick", function (troops, time) {
        updatePopulation(troops);
        updateTimer(time);
    });

    connection.on("Cl_TileUpdate", function (x, y, t_faction, t_population, t_coin) {
        updateTileData(x, y, t_faction, t_population, t_coin);
    });

    connection.on("Cl_CompressedUpdate", function (compressedData) {
        handleCompressedDelta(compressedData);
    });

    connection.on("Cl_GameEnded", function (reason) {
        gameEnded(reason);
        connection.stop();
    });

    connection.on("Cl_Chat", function (sender, msg) {

    });

    connection.on("Cl_ConnectedPlayers", function (players) {
        createPlayerList(players);
    });

    connection.on("Cl_PlayerConnected", function (name, faction) {

    });

    connection.on("Cl_PlayerDisconnected", function (name) {

    });
}