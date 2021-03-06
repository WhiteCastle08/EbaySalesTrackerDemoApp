﻿//Contains the initial screen startup routines
var startup = function () {
    var windowFocused = true,
    init = function (userId) {
        //track if user switches tabs or not otherwise
        //timers may queue up in some browsers like Chrome
        $(window).focus(function () {
            windowFocused = true;
        });

        $(window).blur(function () {
            windowFocused = false;
        });

        //this call will need to produce an object literal that has dynamically calculated the positions for each inventory item
        var defaultPositions = sceneLayoutService.get();


        //sceneStateManager.init(defaultPositions);
        


    };
    return {
        init: init,        
    };
} ();