// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

var faction = "";
var username = "";

var gameMap = [];
var tileMap = [];

var mypopulation = 0;

var capitalPosition;

window.onload = function () {
    if (window.location.href == "https://localhost/") {
        
    }

    this.buildConnection();

    //Load Login form with JQuery
    this.loadLoginForm();
}

function Player() {
    this.id = "";
    this.faction = -1;
}

const Tile = {
    TileType: 'N',
    Population: 0,
    Faction: 'None',
    Coin: 'None'
}

function loadLoginForm() {
    $(".loader-wrapper").fadeIn("10");
    $("#content").load("Login", function () {
        console.log("Load was performed.");
        document.getElementById("btn_login").addEventListener("click", loginBtnClick);
        $(".loader-wrapper").fadeOut("slow");
    });
}

function loginBtnClick() {
    Player.id = document.getElementById("in_username").value;
    Player.faction = faction;
    console.log("Username:" + Player.id);
    console.log("Faction:" + Player.faction);

    if (Player.id == "") {
        alert("Enter a Username");
        return;
    }
    if (Player.faction == "") {
        alert("Select a Faction");
        return;
    }

    connection.invoke("Login", Player.id, "empty",Player.faction).catch(function (err) {
        return console.error(err.toString());
    });
}

function loadGame() {
    $("#content").empty();
    $(".loader-wrapper").fadeIn("100");
    
    $("#content").load("GamePage", function () {
        console.log("Game Loaded");
        document.getElementById("lb_Username").innerHTML = Player.id;
        document.getElementById("lb_Faction").innerHTML = Player.faction;
        tableCreate();
        scrollToCapital();
        $(".loader-wrapper").fadeOut("slow");
    });
}

function createButton(name) {
    var button = document.createElement("input");
    button.type = "button";
    button.value = name;
    button.id = "btnFaction" + name;
    button.style.backgroundColor = name;
    button.onclick = function (event) {
        faction = event.srcElement.value;
        var nodes = document.getElementById("factionButtons").getElementsByTagName('*');
        for (var i = 0; i < nodes.length; i++) {
            nodes[i].disabled = false;
            if (nodes[i].value == event.srcElement.value) {
                nodes[i].disabled = true;
            }
        }
    };
    document.getElementById("factionButtons").appendChild(button);
}

function tableCreate() {
    var parent = document.getElementById("viewport");
    var tbl = document.createElement('table');
    tbl.style.border = 'none';
    tbl.style.position = 'relative';
    tbl.classList.add("dragger");
    var maxX = tileMap.length;
    var maxY = tileMap[0].length;

    for (var i = 0; i < maxX; i++) {
        var tr = tbl.insertRow();
        gameMap.push([]);
        gameMap[i].push(new Array(maxY));
        for (var j = 0; j < maxY; j++) {

            var td = tr.insertCell();

            var tileElem = createTileElement(i, j);

            td.appendChild(tileElem);
            gameMap[i][j] = tileElem;

            td.style.border = 'none';

            addTintToTile(i, j);
        }
    }
    parent.appendChild(tbl);
    // Set drag scroll on first descendant of class dragger on both selected elements
    $('#viewport, #inner').
        dragscrollable({ dragSelector: '.dragger:first', acceptPropagatedEvent: true });
}

function scrollToCapital() {
    //$('#viewport').scrollTo('#' + capitalPosition);
}

function createTileElement(i, j) {

    //Create Divs
    var parentDiv = document.createElement("div");
    parentDiv.classList.add("tileContainer");
    parentDiv.id = i + ":" + j;
    parentDiv.onclick = function (event) {
        gridClicked(event);
    };

    var coinDiv = document.createElement("div");
    coinDiv.classList.add("tileChild");

    var tintDiv = document.createElement("div");
    tintDiv.classList.add("tileChild");

    var structureDiv = document.createElement("div");
    structureDiv.classList.add("tileChild");

    var tileDiv = document.createElement("div");
    tileDiv.classList.add("tileChild");

    var text = document.createElement("p");
    text.innerHTML = "" + tileMap[i][j].Population;
    text.classList.add("tileText");


    switch (tileMap[i][j].TileType) {
        case "N": tileDiv.classList.add("tileNormal"); break;
        case "W": tileDiv.classList.add("tileWater"); break;
        case "F": tileDiv.classList.add("tileForest"); break;
        case "H": tileDiv.classList.add("tileHills"); break;
        case "U": tileDiv.classList.add("tileUrban"); break;
        case "C": tileDiv.classList.add("tileCapital"); break;
        default: tileDiv.classList.add("tileNormal"); console.error("Error occurred loading gameMap - Unknown TileType"); break;
    }
    // </ Button setup >

    //Z-Index
    coinDiv.style.zIndex = "5";
    tintDiv.style.zIndex = "4";
    structureDiv.style.zIndex = "3";
    tileDiv.style.zIndex = "2";
    text.style.zIndex = "6";

    //Append all created elements
    // Structure: child[0] = coinDiv, child[1] = tintDiv, child[2] = structureDiv, child[3] = btn, child[4]
    parentDiv.appendChild(coinDiv);
    parentDiv.appendChild(tintDiv);
    parentDiv.appendChild(structureDiv);
    parentDiv.appendChild(tileDiv);
    parentDiv.appendChild(text);

    return parentDiv;
}

function mapDataToTileMap(mapData) {
    //sizeX:sizeY#Data
    var sizeAndData = mapData.split('#');
    //sizeX:sizeY
    var mapSize = sizeAndData[0].split(':');
    //shortType,Faction,Population;
    var tileData = sizeAndData[1].split(';');
    //forloop to fill DataGrid
    var counter = 0;
    for (var i = 0; i < mapSize[0]; i++) {
        tileMap.push([]);
        tileMap[i].push(new Array(mapSize[1]));
        for (var j = 0; j < mapSize[1]; j++) {
            var tileComp = tileData[counter].split(',');

            var _tile = Object.create(Tile);
            _tile.TileType = tileComp[0];
            _tile.Faction = tileComp[1];
            _tile.Population = tileComp[2];
            _tile.Coin = tileComp[3];
            tileMap[i][j] = _tile;

            if (_tile.TileType == "C" && _tile.Faction == faction) {
                capitalPosition = i + ":" + j;
            }

            counter++;
        }
    }
}

function gridClicked(event) {
    console.log(event);
    var coords = event.srcElement.id.split(':');

    //gameMap[coords[0]][coords[1]].style.backgroundColor = "#2980b9";
    console.log(coords[0] + coords[1] + tileMap[coords[0]][coords[1]].TileType + tileMap[coords[0]][coords[1]].Faction + tileMap[coords[0]][coords[1]].Population);

    connection.invoke("Sv_AttackTile", coords[0], coords[1]).catch(function (err) {
        return console.error(err.toString());
    });
}

function updatePopulation(newPop) {
    document.getElementById("lb_troops").innerHTML = newPop;
    mypopulation = newPop;
}

function addTintToTile(x, y) {
    gameMap[x][y].children[1].classList.remove("tileOverlayRed", "tileOverlayBlue", "tileOverlayGreen", "tileOverlayYellow", "coinBronze", "coinSilver", "coinGold");
    switch (tileMap[x][y].Faction) {
        case "Red": gameMap[x][y].children[1].classList.add("tileOverlayRed"); break;
        case "Blue": gameMap[x][y].children[1].classList.add("tileOverlayBlue"); break;
        default: break;
    }
    switch (tileMap[x][y].Coin) {
        case "Bronze": gameMap[x][y].children[0].classList.add("coinBronze"); break;
        case "Silver": gameMap[x][y].children[0].classList.add("coinSilver"); break;
        case "Gold": gameMap[x][y].children[0].classList.add("coinGold"); break;
        default: break;
    }
}

function updateTileData(x, y, f, p, c) {
    tileMap[x][y].Faction = f;
    tileMap[x][y].Population = p;
    tileMap[x][y].Coin = c;

    gameMap[x][y].children[4].innerHTML = p;

    addTintToTile(x, y);
}

function handleCompressedDelta(base64Data) {
    //Decompress Data
    var compressData = atob(base64Data);
    compressData = compressData.split('').map(function (e) {
        return e.charCodeAt(0);
    });

    var originalText = pako.ungzip(compressData, { to: "string" });

    //Extract Tiledata and display it
    var tileData = originalText.split(';');
    for (var i = 0; i < tileData.length; i++) {
        var _iTE = tileData[i].split(',');
        updateTileData(_iTE[0], _iTE[1], _iTE[2], _iTE[3]);
    }
}