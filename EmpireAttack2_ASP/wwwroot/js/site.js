// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

var faction = "";
var username = "";

var gameMap = [];
var tileMap = [];

var mypopulation = 0;

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
    Faction: 'None'
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

            var btn = document.createElement("input");
            btn.type = "button";
            btn.value = "" + i + "," + j;
            btn.id = i + ":" + j;
            btn.onclick = function (event) {
                gridClicked(event);
                //alert("Grid clicked:" + event.srcElement.id);
            };
            btn.removeAttribute("style");
            btn.classList.add("tile");

            switch (tileMap[i][j].TileType) {
                case "N": btn.classList.add("tileNormal"); break;
                case "W": btn.classList.add("tileWater"); break;
                case "F": btn.classList.add("tileForest"); break;
                case "H": btn.classList.add("tileHills"); break;
                case "U": btn.classList.add("tileUrban"); break;
                case "C": btn.classList.add("tileCapital"); break;
                default: btn.classList.add("tileNormal"); console.error("Error occurred loading gameMap - Unknown TileType"); break;
            }

            td.appendChild(btn);
            gameMap[i][j] = btn;

            td.style.border = 'none';

            addTintToTile(i, j);
        }
    }
    parent.appendChild(tbl);
    // Set drag scroll on first descendant of class dragger on both selected elements
    $('#viewport, #inner').
        dragscrollable({ dragSelector: '.dragger:first', acceptPropagatedEvent: true });
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
            tileMap[i][j] = _tile;

            counter++;
        }
    }
}

function gridClicked(event) {
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
    gameMap[x][y].classList.remove("tileOverlayRed", "tileOverlayBlue", "tileOverlayGreen", "tileOverlayYellow");
    switch (tileMap[x][y].Faction) {
        case "Red": gameMap[x][y].classList.add("tileOverlayRed"); break;
        case "Blue": gameMap[x][y].classList.add("tileOverlayBlue"); break;
        default: break;
    }
}

function updateTileData(x, y, f, p) {
    tileMap[x][y].Faction = f;
    tileMap[x][y].Population = p;

    addTintToTile(x, y);
}