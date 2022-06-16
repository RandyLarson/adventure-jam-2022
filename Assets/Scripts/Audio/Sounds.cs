using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum Sounds {
    ItemPickedUp = 0,
    ItemPlaced = 1,
    MeepleMoves = 2,
    MeepleHappy = 3,
    MeepleSad = 4,
    MeepleJumps = 5,
    ItemsInteract = 6,
    ItemsDontInteract =7,
    ItemTooFarAway =8,
    Walking=9,
    Popping=10,
    RadioStation_Goal=11,
    RadioStation_Other1=12,
    RadioStation_Other2=13,
    RadioStation_Other3=14,
    RadioStation_Changing=15,
    LightSwitch=16,
    DrawerOpen=17,
    DrawerClose=18,
    FridgeOpen=19,
    FridgeClose=20,
    StoveOpen=21,
    StoveClose=22,
    CupboardOpen=23,
    CupboardClose=24,
    CookTimer=25,
    CookTimerDing=26,
    GasStove=27,
    AddToSoup=28,
    TreeGrows=29,
    CatMeow=30,
    CatPurr=31,
    OpenCurtains=32,
    RunningWater=33,
    OpenCan,
    Nothing = 100
}