#nullable disable
using System.ComponentModel;
using System.Diagnostics;

namespace RayCarrot.RCP.Metro;

public class Mod_RRR_MemoryPatcher
{
    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Properties

    public bool autodetectVersion = true;
    public bool isSteam;

    // Main
    public bool enableProtoRaymanEverywhere = true;
    public bool addPlaytestMenu = false; // Replaces game type selection with Playtest menu. Playing with this makes you unable to pause, so include as an option, but it is advised to leave off
    public bool addDuel = true; // Duel replaces Traite des vaches Variante (Bunnies don't milk cows part 2). Play it with multiple players!
    public bool addCameraControls = true; // Essential
    public bool unlockAllMinigames = false;
    public bool setCheatPage = false;
    public int cheatPage = 1; // Range: 0 (off), 1-5 (on, different debug info)

    // Rayman
    public bool addLookMode = true;  // Add a look mode on LT. This was disabled in the code with "if(button) lookmode = true; lookmode = false;"
    public bool climbWalls = false; // Sacrifices some less important hook code for wall climbing behavior
    public bool groovyRaymanDanceMoveset = false; // Rayman gets a groovy dance moveset
    public bool addBoostButton = false; // Normally reserved to items with a certain type, this mode speed up Rayman with RT.
    public bool lowerSlippery = false; // Doubles default friction value for Rayman
    public bool setPlayer1Costume = true;  // Always configures player 1's costume on Rayman, even when the player isn't defined
    public bool controlTempo = true;  // In dance mode, holding LB will start an internal beat for Rayman to dance to
    public bool hangFromHotspots = false; // Makes the hook not work as well, but an early system presumably for rings is used to hang from hooks
    public bool addFinishers = true; // Hold or press RB to add finishers into your combos
    public bool setGrappinGFX = false;  // If set, the hook's graphics are changed
    public byte GrappinGFX = 0;  // If setGrappinGFX is set, the hook's graphics are changed (0=Line, 1=Lightning, 2=Sparks, 3=Corde, 4=Line_Tiptop)
    public bool immortal = false; // Rayman doesn't die
    public bool allpowers = false; // The powerup value is barely used anymore, but this used to set flags for being able to use certain mounts, having a longer hook etc. Now it's just longer helicopter (game names it infinite but it isn't) + infinite wall climb
    public bool disableMinigameIntro = false; // Minigame intro cinematics change the state of the world and sometimes deactivate Rayman
    public bool disableFootstepSound = false; // Rayman's soundbank is nulled in all levels aside from the prison cell & the arena... and the carrot juice game
    public bool drawHealthMana = false;
    public bool noInstaKill = true;

    // Rabbids
    public bool rabbidsDropItems = false; // Super nice, but should be an option
    public bool rabbidsIncreasedHP = false; // 100 HP for each rabbid? Madness!
    public bool randomProtoRabbidPowers = false; // With a 1/10 chance each, rabbids are given one of these powers: suck, blow, mole, get back up

    // Mounts
    public bool makeRhinosAggressive = false; // Chaos
    public bool makeQuadriIntoSpiders = false; // The spiders just get bigger, not really special. Causes crashes, so leave off
    public bool makeBatsIntoEagles = false; // Feels fantastic. So majestic
    public bool makePigsIntoPlums = false; // Pigs become bouncing plums that you can throw on the rabbids' heads. Nice.
    public bool tameMounts = true;  // Essential
    public bool saucersStartFlying = false; // Nicer if they start landed, but an option
    public bool makeSpidersJump = false; // Not worth it, barely gets off the floor.
    public bool makeBatShoot = true;  // Enabled by default since it's harmless. Bat shoots with the attack button
    public bool makeSoucoupeShoot = false; // This one shouldn't be an option, leave false

    // FPS & Triggers
    public bool missileLaunchersTargetPlayer = false; // Can make exploration rather difficult since these are a 1 hit KO, but nice
    public bool activateAllActivatorTriggers = false; // Chaos
    public bool dontDestroyBipods = false;
    public bool activateAllPivotInBVTriggers = false; // Causes crashes in some levels, but spawns more rabbids in others

    // Keyboard controls
    public bool fixKeyboardControls = true;

    // Leave out
    public bool mergeAllSBWithRayman = false; // (Leave off) Merge all soundbanks with Rayman's. Fixes his footstep sound in FPS levels but also plays even more annoying grunts
    public bool disableMinigame = false; // Crashes easily, but disables everything

    // unused
    /*bool markFightTargets = false;
    bool longerHook = true;  // Rayman's hook can reach twice as far
    bool moreLife = true;
    bool moreMana = true;
    bool addCheatCheck = true;*/

    // DEBUG
    //public bool readContinous = false;
    //public bool continuous = false; // Crashes easily

    #endregion

    #region Patch

    public void Patch()
    {
        IntPtr processHandle = GetRRRProcessHandle();

        try
        {
            var handle = (int)processHandle;

            if (autodetectVersion)
                DetectVersion(handle);

            if (enableProtoRaymanEverywhere)
            {
                EnableRaymanInFPS(handle);
                DestroyAnnoyingAI(handle);
                DisableSectos(handle);
                AddRaymanCheatToggle(handle);
                ImproveFPSTriggers(handle);
                AddDanceSystem(handle);
                MountStuff(handle);
                LapinChanges(handle);
                EnableRaymanMoveset(handle);
                DisableMinigameStuff(handle);
                    
                if (fixKeyboardControls) 
                    FixKeyboardControls(handle);
            }
            if (addCameraControls)
                AddCameraControls(handle);

            if (addPlaytestMenu)
                AddPlaytestMenu(handle);

            if (addDuel)
                AddDuel(handle);

            DebugCheats(handle);

            //IntMIG_AFaireToutLeTempsAUDEBUT__AddCheatCheck.Apply(processHandle, isSteam);

            /* Continuous_ForceSectoMode(processHandle);
             Continuous_SetUniversVars(processHandle);*/

            // Debug
            //if (continuous || readContinous)
            //{
            //    do
            //    {
            //        while (!Console.KeyAvailable)
            //        {
            //            System.Threading.Thread.Sleep(500);
            //            if (continuous)
            //            {
            //                Continuous_SetUniversVars(processHandle);
            //                Continuous_SetRaymanVars(processHandle);
            //                Continuous_ForceSectoMode(processHandle);
            //            }
            //            else if (readContinous)
            //            {
            //                Continuous_Read(processHandle);
            //            }
            //            System.Threading.Thread.Sleep(500);
            //        }
            //    }
            //    while (Console.ReadKey().Key != ConsoleKey.P);
            //}
        }
        finally
        {
            Mod_RRR_MemoryManager.CloseHandle(processHandle);
        }
    }

    private void DetectVersion(int processHandle)
    {
        int aeInitAddress = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, FctDefPointer(Mod_RRR_AI2C_fctDef.AE_Init));
        switch (aeInitAddress)
        {
            case off_AE_Init_GOG:
                Logger.Info("RRR Memory Mod: Version detection found GOG version");
                isSteam = false;
                break;

            case off_AE_Init_Steam:
                Logger.Info("Version detection found Steam version");
                isSteam = true;
                break;

            default:
                throw new NotImplementedException("Only the GOG and Steam versions are supported");
        }
    }

    private void AddPlaytestMenu(int processHandle)
    {
        // Read Address for FPS triggers' exec state, then replace wait state with it
        int execState = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, FctDefPointer(Mod_RRR_AI2C_fctDef.IntMIG_Page_Playtest));
        Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, FctDefPointer(Mod_RRR_AI2C_fctDef.IntMIG_Page_GameType), execState);
        IntMIG_Page_MapList__ChangeBinCheck.Apply(processHandle, isSteam);
    }

    private void AddRaymanCheatToggle(int processHandle)
    {
        raym_exec_read_joy__AddCheatModeToggle.Apply(processHandle, isSteam);
        //Memory.WriteProcessMemoryInt32(processHandle, addr_WOR_currentConsole, 10);
    }

    private void AddCameraControls(int processHandle)
    {
        KamP_JoyD_Get__EnableCameraControl_AllModes.Apply(processHandle, isSteam);
        KamP_JoyD_Get__EnableCameraControl_RightStick.Apply(processHandle, isSteam);
    }

    private void AddDanceSystem(int processHandle)
    {
        MMa_Go__AddGroovyBabyToggle.Apply(processHandle, isSteam);

        if (controlTempo)
        {
            //Proc_RM_FullDeguisementGet__Fix.Apply(processHandle, isSteam);
            Proc_RM_FullDeguisementGet__Fix1.Apply(processHandle, isSteam);
            Proc_RM_FullDeguisementGet__Fix2.Apply(processHandle, isSteam);
            Proc_RM_FullDeguisementGet__Fix3.Apply(processHandle, isSteam);
            Proc_RM_FullDeguisementGet__Fix4.Apply(processHandle, isSteam);
            Proc_SND_Groovy_Beat__SkipTempoChecks.Apply(processHandle, isSteam);
            Proc_MM_SND_BeatManager__SetTempo_1.Apply(processHandle, isSteam);
            Proc_MM_SND_BeatManager__SetTempo_2.Apply(processHandle, isSteam);
        }

        Lapin_track_reflex__ModifyBeatCheck.Apply(processHandle, isSteam);

        // Use groovy moveset (bytes differ based on whether it is used or not)
        raym_ETAT_danse__GroovyMoveset_01.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_02.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_03.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_04.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_05.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_06.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_07.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_08.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_09.Apply(processHandle, isSteam);
        raym_ETAT_danse__GroovyMoveset_10.Apply(processHandle, isSteam);
    }

    private void Continuous_SetRaymanVars(int processHandle)
    {
        int off_raymanBuffer = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_raymanbuffer_ptr);
        //Console.WriteLine($"{off_raymanBuffer:X8}");
        foreach (var var in RMVariables_int)
        {
            int read = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_raymanBuffer + var.Key);
            if (read != var.Value)
            {
                int written = Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, off_raymanBuffer + var.Key, var.Value);
            }
            //Console.WriteLine(written);
        }
        foreach (var var in RMVariables_float)
        {
            float read = Mod_RRR_MemoryManager.ReadProcessMemoryFloat(processHandle, off_raymanBuffer + var.Key);
            if (read != var.Value)
            {
                int written = Mod_RRR_MemoryManager.WriteProcessMemoryFloat(processHandle, off_raymanBuffer + var.Key, var.Value);
            }
            //Console.WriteLine(written);
        }
    }

    private void Continuous_Read(int processHandle)
    {
        int off_raymanBuffer = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, 0x0085B5C4);
        int read2 = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_raymanBuffer + 2928);
        int off_mma = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, 0x0085AA94);
        float read = Mod_RRR_MemoryManager.ReadProcessMemoryFloat(processHandle, off_mma + 0xFC);
        //Console.WriteLine($"SND tempo: {read2} - {read2:X8} - Groovy: {read}");
    }

    private void EnableRaymanMoveset(int processHandle)
    {
        MM_InitFin__EnableRaymanPowers.Apply(processHandle, isSteam);
        MM_InitFin__EnableRaymanFight.Apply(processHandle, isSteam);
        GS_exec_SetMicro__SkipPresetMicroCheck.Apply(processHandle, isSteam);
        if (addLookMode) raym_exec_read_joy__AddLookMode.Apply(processHandle, isSteam);
        if (climbWalls)
        {
            raym_exec_grappin__HookWalls1.Apply(processHandle, isSteam);
            raym_exec_grappin__HookWalls3_SetGrappinNormale.Apply(processHandle, isSteam);
            raym_exec_grappin__HookWalls2_SetVarapPos.Apply(processHandle, isSteam);
            raym_exec_grappin__HookWalls4.Apply(processHandle, isSteam);
        }
        if (addFinishers)
        {
            MOVE_CHANGE_To_Punch__Finishers_1.Apply(processHandle, isSteam);
            raym_ETAT_main__Finishers_2.Apply(processHandle, isSteam);
        }

        if (hangFromHotspots) PROC_RM_Type_Hotspot__MakeRope.Apply(processHandle, isSteam);
        if (disableFootstepSound)
        {
            raym_exec_SND__DisableFootstep.Apply(processHandle, isSteam);
            //DisableFctDef(processHandle, AI2C_fctDef.SB_Init);
        }
        if (mergeAllSBWithRayman)
        {
            SB_Init__MergeAllSoundbanksWithRaymans.Apply(processHandle, isSteam);
        }
        if (addBoostButton)
        {
            //raym_exec_read_joy__Deguisement_RemoveCapaCheck.Apply(processHandle, isSteam);
            raym_exec_read_joy__RemoveBoostCheck_1.Apply(processHandle, isSteam);
            raym_exec_read_joy__RemoveBoostCheck_2.Apply(processHandle, isSteam);
        }
        if (lowerSlippery) raym_init__ChangeFriction.Apply(processHandle, isSteam);
        if (drawHealthMana)
        {
            MG_Traire_ETAT_MiniGame__DrawFloat.Apply(processHandle, isSteam);
            raym_track_end__CallDrawFloat1.Apply(processHandle, isSteam);
            raym_track_end__CallDrawFloat2.Apply(processHandle, isSteam);
        }
        if (noInstaKill)
        {
            MG_Traire_reflex__AddHitFunction.Apply(processHandle, isSteam);
            raym_ETAT_paf__RemoveInstaKill.Apply(processHandle, isSteam);
        }
    }

    private void DisableMinigameStuff(int processHandle)
    {
        if (setPlayer1Costume) GST_MiniGame_Player_ShapeSet__SetPlayerCostume.Apply(processHandle, isSteam);
        if (disableMinigameIntro)
        {
            GST_MiniGame_Init__DeactivateIntro_1.Apply(processHandle, isSteam);
        }
    }

    private void DisableSectos(int processHandle)
    {
        OBJ_GameObjectCallback__ForceSectosToZero_1.Apply(processHandle, isSteam);
        OBJ_GameObjectCallback__ForceSectosToZero_2.Apply(processHandle, isSteam);
        AI_EvalFunc_SCT_SetOf_C__ForceSectosToZero_3.Apply(processHandle, isSteam);
    }

    private void DestroyAnnoyingAI(int processHandle)
    {
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Ray_FPS_track_init);

        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Fake_Ray_Course_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Fake_Ray_Throw_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Fake_Ray_JumpingRope_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Fake_Ray_Fighter_Init);
        //DisableFctDef(processHandle, AI2C_fctDef.GST_Dance_INIT);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Fake_Toilette_Zone_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Traire_init);
        //DisableFctDef(processHandle, AI2C_fctDef.MG_BatPig_Track_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.RM_ChuteLibre_Track_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Tondre_ETAT_wait);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Wii_Cursor_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.KM_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Montures_Manager_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Foot_Rayman_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.mgrl_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Demineur_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Wii_FormRecognition_Init);
        //DisableFctDef(processHandle, AI2C_fctDef.MG_ClockTimer_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_GrappinSoucoupe_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_MasterMind_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Curling_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.Fake_CatchManager_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_123Soleil_Init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Maillard_Track_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Marteau_Track_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Punaise_Track_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_Simon_Track_init);
        DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.MG_ThrowingAxe_Init);

        //DisableFctDef(processHandle, AI2C_fctDef.ES_EnterMap);
        if (disableMinigame) DisableFctDef(processHandle, Mod_RRR_AI2C_fctDef.GST_MiniGame_Init);
        //DisableFctDef(processHandle, AI2C_fctDef.mkit_init);
    }

    private void DisableFctDef(int processHandle, Mod_RRR_AI2C_fctDef function)
    {
        Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, FctDefPointer(function), off_AE_Init);
    }

    private int FctDefPointer(Mod_RRR_AI2C_fctDef function) => off_AI2C_fctDefs + (int)function * 12 + 4;

    private void ImproveFPSTriggers(int processHandle)
    {
        GST_FPS_MP_ETAT_exec__SkipTriggerCheck.Apply(processHandle, isSteam);
        TrigTest_PivotInBV__SetGAO2ToRayman.Apply(processHandle, isSteam);
        if (dontDestroyBipods)
        {
            PNJ_Bipod_Init__DontDestroy.Apply(processHandle, isSteam);
            PNJ_Bipod_ETAT_Basic__DontDestroy.Apply(processHandle, isSteam);
            PNJ_Bipod_Reflex_DontPaf.Apply(processHandle, isSteam);
        }
        if (missileLaunchersTargetPlayer) RM_MissileLauncher_Init__SetTargetToRayman.Apply(processHandle, isSteam);
        if (activateAllActivatorTriggers) ActivatorTrigger_wait__SkipTriggerCheck.Apply(processHandle, isSteam);
        if (activateAllPivotInBVTriggers)
        {
            TrigTest_PivotInBV__AlwaysTrue.Apply(processHandle, isSteam);
            //TrigTest_List__ReplacePivotInBV.Apply(processHandle, isSteam);
            //TrigTest_List__ReplacePivotInOneBV.Apply(processHandle,isSteam);
        }

        // Read Address for FPS triggers' exec state, then replace wait state with it
        int execState = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, FctDefPointer(Mod_RRR_AI2C_fctDef.GST_FPS_MP_ETAT_exec));
        Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, FctDefPointer(Mod_RRR_AI2C_fctDef.GST_FPS_MP_ETAT_wait), execState);
    }

    private void LapinChanges(int processHandle)
    {
        if (rabbidsIncreasedHP) Lapin_track_init__IncreaseMaxLife.Apply(processHandle, isSteam);
        if (rabbidsDropItems) Lapin_ITEM_Destroy__DropItems.Apply(processHandle, isSteam);
        if (randomProtoRabbidPowers)
        {
            Lapin_track_init__RandomProtoPowers1.Apply(processHandle, isSteam);
            Lapin_track_init__RandomProtoPowers2.Apply(processHandle, isSteam);
            Proc_PNJ_Lapin_DANSE_Init__RandomProtoPowers.Apply(processHandle, isSteam);
        }

        var test = Lapin_track_init__Test;
        if (test.Bytes.Length > 0) test.Apply(processHandle, isSteam);
    }

    private void MountStuff(int processHandle)
    {
        // Force rabbids off mount
        Lapin_track_init__ForceOffMount.Apply(processHandle, isSteam);

        // Allow Rayman to dismount rhino & bat
        RM_Mount_Jump_OFF__RemoveIDCheck.Apply(processHandle, isSteam);

        // Disable bat setting Rayman's position
        PNJ_Volant_callback_afterblend__RemoveRaymanPositionChange.Apply(processHandle, isSteam);
        if (makeSoucoupeShoot) PNJ_Volant_Shoot__MakeSoucoupeShoot.Apply(processHandle, isSteam);

        // Allow spiders to jump
        if (makeSpidersJump) PNJ_Spider_ETAT_Basic__AllowJumpForQuadri.Apply(processHandle, isSteam);

        // Make PNJ_Spider Quadri models use spider behavior
        if (makeQuadriIntoSpiders)
        {
            PNJ_Spider_Init__UseSpiderType.Apply(processHandle, isSteam);
            PNJ_Spider_Init_IK__Use4LegsForSpiderType.Apply(processHandle, isSteam);
            PNJ_Spider_RidedPosGet__FixGaoCanalForSpider.Apply(processHandle, isSteam);
            //PNJ_Spider_Init__HalfSizeForSpiderType.Apply(processHandle, isSteam);
        }

        // Make PNJ_Volant Bat models use eagle behavior
        if (makeBatsIntoEagles) PNJ_Volant_init__UseEagleTypeForBat.Apply(processHandle, isSteam);

        // Make PNJ_Prune Pig models use plum behavior
        if (makePigsIntoPlums) PNJ_Prune_init__UsePlumTypeForPig_Type0.Apply(processHandle, isSteam);

        if (tameMounts)
        {
            // Remove spiders' reference to Waypoint network when jumping on, making them controllable
            PNJ_Spider_Reflex__RemoveNextWP.Apply(processHandle, isSteam);

            // Start saucers out of their "FPS mode"
            if (saucersStartFlying)
            {
                // in the "balade" state
                PNJ_Volant_Init__StartOutOfFPSMode_Flying.Apply(processHandle, isSteam);
            }
            else
            {
                // in the "basic" state
                PNJ_Volant_Init__StartOutOfFPSMode_Parked.Apply(processHandle, isSteam);
            }
        }
        // Set agressive flag in rhinos
        if (makeRhinosAggressive) PNJ_Quadri_Init__MakeAgressive.Apply(processHandle, isSteam);
    }

    private void EnableRaymanInFPS(int processHandle)
    {
        /*int off_rayfps_to_rayman = Memory.ReadProcessMemoryInt32(processHandle, off_rayfps_to_rayman_ptr);
        if (off_rayfps_to_rayman != 0) {
            int rayman_controlFlags = Memory.ReadProcessMemoryInt32(processHandle, off_rayfps_to_rayman + 4);
            Console.WriteLine($"{off_rayfps_to_rayman:X8} - {rayman_controlFlags:X8}");
            rayman_controlFlags &= ~0x00200000;
            int written = Memory.WriteProcessMemoryInt32(processHandle, off_rayfps_to_rayman + 4, rayman_controlFlags);
        }*/

        // Patch type_game number. Always set to 0 (setting it to other values like 666, the FPS mode value, can disable Rayman)
        MM_InitFin__ForceTypeGameToZero.Apply(processHandle, isSteam);
    }

    private void Continuous_ForceSectoMode(int processHandle)
    {
        // Forces secto mode to 1 (= sector changes controlled by the camera)
        int read = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_SCT_gul_Flags);
        if (read != 1)
        {
            int written = Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, off_SCT_gul_Flags, 1);
        }
    }

    private void Continuous_SetUniversVars(int processHandle)
    {
        int off_univBuffer = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_univers_ptr);
        //Console.WriteLine($"{off_raymanBuffer:X8}");
        foreach (var var in UniversVariables_Int)
        {
            int read = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_univBuffer + var.Key);
            if (read != var.Value)
            {
                int written = Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, off_univBuffer + var.Key, var.Value);
            }
            //Console.WriteLine(written);
        }
    }

    private void AddDuel(int processHandle)
    {
        int off_univBuffer = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_univers_ptr);
        foreach (var var in UniversVariables_Duel_Int)
        {
            int read = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_univBuffer + var.Key);
            if (read != var.Value)
            {
                int written = Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, off_univBuffer + var.Key, var.Value);
            }
            //Console.WriteLine(written);
        }
    }

    private readonly Dictionary<int, int> UniversVariables_Cheat_Int = new Dictionary<int, int>()
    {
        [0x000080C4] = 1, // i_cheat_all_unlocked
        [0x000080D0] = -1, // i_FinalCheat_Unlocked
    };
    private int UniversVariables_CheatPage => 0x00001574;

    private void DebugCheats(int processHandle)
    {
        int off_univBuffer = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_univers_ptr);

        if (unlockAllMinigames)
        {
            foreach (var var in UniversVariables_Cheat_Int)
            {
                int read = Mod_RRR_MemoryManager.ReadProcessMemoryInt32(processHandle, off_univBuffer + var.Key);

                if (read != var.Value)
                    Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, off_univBuffer + var.Key, var.Value);
            }
        }
            
        if (setCheatPage)
            Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, off_univBuffer + UniversVariables_CheatPage, cheatPage);
    }
    private void FixKeyboardControls(int processHandle)
    {
        SetWorldKeyMapping__FixKeyboardControls1.Apply(processHandle, isSteam);
        SetWorldKeyMapping__FixKeyboardControls2.Apply(processHandle, isSteam);
        SetWorldKeyMapping__FixKeyboardControls3.Apply(processHandle, isSteam);
        CheckControlMode_Gladiator__FixKeyboardControls4.Apply(processHandle, isSteam);
            
        var keycodes = KeyboardKeycodes;

        for (int i = 0; i < keycodes.Length; i++)
            Mod_RRR_MemoryManager.WriteProcessMemoryInt32(processHandle, addr_KeyboardKeycodes + i * 4, keycodes[i]);
    }

    private static IntPtr GetRRRProcessHandle()
    {
        var processNames = new string[]
        {
            "Jade_enr",
            "Jade_enr.exe",
        };

        Process process = processNames.
            Select(Process.GetProcessesByName).
            Where(processes => processes.Any()).
            Select(processes => processes.First()).
            FirstOrDefault();

        if (process == null)
            throw new Exception("Game process was not found running");

        IntPtr processHandle = Mod_RRR_MemoryManager.OpenProcess(Mod_RRR_MemoryManager.PROCESS_ALL_ACCESS, false, process.Id);

        if (processHandle == IntPtr.Zero)
            throw new Win32Exception();

        return processHandle;
    }

    #endregion

    #region Univers
    private int off_univers_ptr => isSteam ? 0x0085B600 : 0x0085B620;

    private Dictionary<int, int> UniversVariables_Int = new Dictionary<int, int>()
    {
        [0x00002540] = -1, // i_RM_powerUP
        [0x000080C0] = -1, // i_cheat_walkthrough
        /*[0x0000255C] = 15, // ai_disguise[0]
        [0x00002560] = 15, // ai_disguise[1]
        [0x00002564] = 15, // ai_disguise[2]
        [0x00002568] = 15, // ai_disguise[3]
        [0x0000256C] = 15, // ai_disguise[4]
        [0x00002570] = 15, // ai_disguise[5]*/
    };
    private Dictionary<int, int> UniversVariables_Duel_Int = new Dictionary<int, int>()
    {
        [652 + Duel_LevelIndexToReplace * 4] = -587196423, // Key for Duel: 0xDD0017F9. Duel is normally index 92.
    };
    private const int Duel_LevelIndexToReplace = 75; // Index for Traite des vaches variante

    private int addr_WOR_currentConsole => isSteam ? 0x0086EAA8 : 0x0086EAC8;

    /*int off_rayfps_to_rayman_ptr = 0x0085B6BC;
    int off_curgameObject_ptr = 0x00859ACC;
    int off_code_rayID_ptr = 0x006BCAE0;*/
    #endregion

    #region Sectors
    // Force disable sectors (necessary for FPS levels)
    private Mod_RRR_MemoryPatch OBJ_GameObjectCallback__ForceSectosToZero_1 => new Mod_RRR_MemoryPatch(0x00724805, 0x00724895, new byte[] {
        0x90, 0x90, 0x90, 0x90 // nop * 4
    });

    private Mod_RRR_MemoryPatch OBJ_GameObjectCallback__ForceSectosToZero_2 => new Mod_RRR_MemoryPatch(0x00724834, 0x007248C4, new byte[] {
        0x90, 0x90, 0x90, 0x90 // nop * 4
    });

    private Mod_RRR_MemoryPatch AI_EvalFunc_SCT_SetOf_C__ForceSectosToZero_3 => new Mod_RRR_MemoryPatch(0x00416FB0, 0x00417020, new byte[] {
        0xC3, 0xCC, 0xCC, 0xCC // retn + 3 bytes of padding
    });

    private int off_SCT_gul_Flags => isSteam ? 0x86EA68 : 0x0086EA88;
    #endregion

    #region Rayman
    // If the game type is not zero, Rayman will sometimes be disabled
    private Mod_RRR_MemoryPatch MM_InitFin__ForceTypeGameToZero => new Mod_RRR_MemoryPatch(0x00697910, 0x00697950, new byte[] {
        0xB8, 0x00, 0x00, 0x00, 0x00, // mov eax, 0
        0x90                          // nop
    });

    private Mod_RRR_MemoryPatch MM_InitFin__EnableRaymanFight => new Mod_RRR_MemoryPatch(0x0069791E, 0x0069795E, new byte[] {
        0xB8, 0x01, 0x00, 0x00, 0x00,
        0x89, 0x81, 0xE4, 0x11, 0x00, 0x00, // mov     [ecx+11E4h], eax
        0xC7, 0x81, 0xE8, 0x09, 0x00, 0x00, (byte)(setGrappinGFX ? GrappinGFX : 0x03), 0x00, 0x00, 0x00, // i_grappin_gfx_mode
        0xC7, 0x81, 0x08, 0x0E, 0x00, 0x00, (byte)(immortal ? 0x02 : 0x00), 0x00, 0x00, 0x00, // i_cheat_immortel
    }.Concat(allpowers ? new byte[] {
        0xC7, 0x82, 0x40, 0x25, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, // Univers+0x2540 (i_RM_powerUP): Set powerup value
    } : new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
    }).Concat(new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
    }).ToArray());

    private Mod_RRR_MemoryPatch MM_InitFin__EnableRaymanPowers => new Mod_RRR_MemoryPatch(0x0069795E, 0x0069799E, new byte[] {
        0xBA, 0x01, 0x00, 0x00, 0x00, 0x90, 0x89, 0x91, 0xD0, 0x11, 0x00, 0x00, // i_RM_Move = 1
        0xB8, 0x01, 0x00, 0x00, 0x00, 0x90, 0x89, 0x81, 0xD4, 0x11, 0x00, 0x00, // i_RM_Grappin = 1
        0xBA, 0x01, 0x00, 0x00, 0x00, 0x90, 0x89, 0x91, 0xD8, 0x11, 0x00, 0x00, // i_RM_Jump = 1
        0xB8, 0x01, 0x00, 0x00, 0x00, 0x90, 0x89, 0x81, 0xDC, 0x11, 0x00, 0x00, // i_RM_Roulade = 1
        0xBA, 0x00, 0x00, 0x00, 0x00, 0x90, 0x89, 0x91, 0xCC, 0x11, 0x00, 0x00, // i_RM_Light = 0
        0xB8, 0x01, 0x00, 0x00, 0x00, 0x90, 0x89, 0x81, 0xE0, 0x11, 0x00, 0x00, // i_RM_Deguisement = 1
        //0xBA, 0x00, 0x00, 0x00, 0x00, 0x90 // Set Rayman's item to 0
    });

    private Mod_RRR_MemoryPatch raym_exec_read_joy__AddCheatModeToggle => new Mod_RRR_MemoryPatch(0x0067906B, 0x0067905B, new byte[] {
        0x6A, io_button_noclip,
        0xE8, (byte)(isSteam ? 0xFE : 0xBE),  (byte)(isSteam ? 0xEF : 0xF0), 0xD8, 0xFF,
        0x83, 0xC4, 0x04,
        0x85, 0xC0,
        0xBF, 0x01, 0x00, 0x00, 0x00,
        0x74, 0x10,
        0x8B, 0x86, 0xF4, 0x0D, 0x00, 0x00,
        0x89, 0xFA,
        0x29, 0xC2,
        0x89, 0x96, 0xF4, 0x0D, 0x00, 0x00,
        0x8B, 0x86, 0xF4, 0x0D, 0x00, 0x00,
        0x83, 0xC4, 0x08,
        0x39, 0xD8,
        0x0F, 0x85, 0xA2, 0x09, 0x00, 0x00
    });

    private Mod_RRR_MemoryPatch raym_exec_read_joy__AddLookMode => new Mod_RRR_MemoryPatch(0x00679140, 0x00679130, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
    });

    /*MemoryPatch raym_exec_read_joy__Deguisement_RemoveCapaCheck => new MemoryPatch(0x0067924C, 0x0067923C, new byte[] {
        0x90, 0x90,
    });*/
    private Mod_RRR_MemoryPatch raym_exec_read_joy__RemoveBoostCheck_1 => new Mod_RRR_MemoryPatch(0x00679168, 0x00679158, new byte[] {
        //0xC7, 0x86, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x16, 0x43, // Sets boost speed to 150f
        0xC7, 0x86, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, (byte)(lowerSlippery ? 0x96 : 0x16), (byte)(lowerSlippery ? 0x42 : 0x43), // Sets boost speed to 75f (for modded friction) or 150 (og friction)
        /*0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,*/ 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
    });

    private Mod_RRR_MemoryPatch raym_exec_read_joy__RemoveBoostCheck_2 => new Mod_RRR_MemoryPatch(0x006791BF, 0x006791AF, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
    });

    // Allows you to hook walls and climb. However this sacrifices other parts of the hook behaviour.
    private Mod_RRR_MemoryPatch raym_exec_grappin__HookWalls1 => new Mod_RRR_MemoryPatch(0x0067F1B5, 0x0067F1A5, new byte[] {
        0xEB, 0x44,
    });

    private Mod_RRR_MemoryPatch raym_exec_grappin__HookWalls2_SetVarapPos => new Mod_RRR_MemoryPatch(0x0067F208, 0x0067F1F8, new byte[] {
        //0xC7, 0x86, 0xEC, 0x08, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90, 0x8B,
        0x89, 0x8E, 0x74, 0x06, 0x00, 0x00,
        0x90,
        0x8B, 0x50, 0x04,
        0x89, 0x96, 0x78, 0x06, 0x00, 0x00,
        0x90,
        0x8B, 0x40, 0x08,
        0x8D, 0x4C, 0x24, 0x30,
        0x57,
        0x51,
        0x89, 0x86, 0x7C, 0x06, 0x00, 0x00,
        0x90,
    });

    private Mod_RRR_MemoryPatch raym_exec_grappin__HookWalls3_SetGrappinNormale => new Mod_RRR_MemoryPatch(0x0067F230, 0x0067F220, new byte[] {
        //0xC7, 0x86, 0xEC, 0x08, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90, 0x8B,
        0x89, 0x96, 0x8C, 0x09, 0x00, 0x00,
        0x8B, 0x48, 0x04,
        0x89, 0x8E, 0x90, 0x09, 0x00, 0x00,
        0x8B, 0x50, 0x08,
        0x89, 0x96, 0x94, 0x09, 0x00, 0x00,
        0x68, 0xFF, 0x00, 0x00, 0x7F,
        0x8D, 0x84, 0x24, 0x88, 0x00, 0x00, 0x00,
        0x50,
        0x8D, 0x8C, 0x24, 0xB0, 0x00, 0x00, 0x00,
        0x83, 0xC4, 0x18,
        0xC7, 0x86, 0xEC, 0x08, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0xE9, 0x55+5, 0x06, 0x00, 0x00,
    });

    private Mod_RRR_MemoryPatch raym_exec_grappin__HookWalls4 => new Mod_RRR_MemoryPatch(0x04EBDAC, 0x004EBF3C, new byte[] {
        0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,
    });

    private Mod_RRR_MemoryPatch MOVE_CHANGE_To_Punch__Finishers_1 => new Mod_RRR_MemoryPatch(0x004EB546, 0x004EB6D6, new byte[] {
        0xB8, io_button_finisher, 0x00, 0x00, 0x00, // mov     eax, io_button_finisher
        0x50, // push eax
        0xE8, (byte)(isSteam ? 0x1F : 0x3F), (byte)(isSteam ? 0xC5 : 0xC4), 0xF1, 0xFF, // call    AI_EvalFunc_IoButtonPressed_C
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, // nop x 9
    });

    private Mod_RRR_MemoryPatch raym_ETAT_main__Finishers_2 => new Mod_RRR_MemoryPatch(0x006D7D20, 0x006D7D60, new byte[] {
        0xB8, io_button_finisher, 0x00, 0x00, 0x00, // mov     eax, io_button_finisher
        0x89, 0xAE, 0xC4, 0x0D, 0x00, 0x00, // mov     dword ptr [esi+0DC4h], ebp
        0x50, // push eax
        0xE8, (byte)(isSteam ? 0x3F : 0xAF), 0xFD, 0xD2, 0xFF, // call    AI_EvalFunc_IoButtonPressed_C
        0x90, 0x90, 0x90, 0x90, 0x90, // nop x 5
    });

    /*MemoryPatch raym_track_reflex__AddTargetingSystem1 => new MemoryPatch(0, 0x006D9B2E, new byte[] {
        0x8B, 0x86, 0x58, 0x09, 0x00, 0x00, // mov     eax, [esi+958h]
        0x85, 0xC0,                         // test eax, eax
        0x74, 0x08,                         // jmp (just past setting the value)
        0x90, 0x90,                         // nop x 2
        0x89, 0x86, 0xD8, 0x0C, 0x00, 0x00  // mov     [esi+0CD4h], eax
    });
    MemoryPatch raym_track_reflex__AddTargetingSystem2 => new MemoryPatch(0, 0x006D9B4D, new byte[] {
        0x83, 0xC4, 0x08 // add esp, 8
    });
    MemoryPatch raym_AddTargetingSystem3 => new MemoryPatch(0, 0x004EB6EF, new byte[] {
        0x90, 0x90
    });
    MemoryPatch raym_AddTargetingSystem4 => new MemoryPatch(0, 0x006D7D7B, new byte[] {
        0x90, 0x90
    });*/

    private Mod_RRR_MemoryPatch PROC_RM_Type_Hotspot__MakeRope => new Mod_RRR_MemoryPatch(0x004ED8A4, 0x004EDA34, new byte[] {
        0x90, 0x90
    });

    private Mod_RRR_MemoryPatch KamP_JoyD_Get__EnableCameraControl_AllModes => new Mod_RRR_MemoryPatch(0x005D8E2E, 0x005D8E2E, new byte[] {
        0x90, 0x90
    });

    private Mod_RRR_MemoryPatch KamP_JoyD_Get__EnableCameraControl_RightStick => new Mod_RRR_MemoryPatch(0x005D8E30, 0x005D8E30, new byte[] {
        0xE8, (byte)(isSteam ? 0xEB : 0x9B), (byte)(isSteam ? 0xF0 : 0xF1), 0xE2, 0xFF,             // call AI_EvalFunc_IoJoyGetMove1_C_AI2C__Fv
        /*
        0x83, 0xC4, 0x04,                         // add esp, 4
        0xC7, 0x46, 0x10, 0x01, 0x00, 0x00, 0x00, // mov dword ptr [esi+10h], 0
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90,       // nop x6
        */
    });

    private Mod_RRR_MemoryPatch raym_init__ChangeFriction => new Mod_RRR_MemoryPatch(0x006CA0EC, 0x006CA12C, new byte[] {
        0x68, 0x00, 0x00, 0x00, 0x41, // Default value = 4 ( 00 00 80 40 )
        0x68, 0x00, 0x00, 0x00, 0x41, // Modded value = 8 ( 00 00 00 41 )
        0xC7, 0x86, 0xF8, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41
    });

    private Mod_RRR_MemoryPatch MG_Traire_reflex__AddHitFunction => new Mod_RRR_MemoryPatch(0x006868C0, 0x006868B0, new byte[] {
        0x50,                                                        // push    eax
        0x56,                                                        // push    esi
        0x8B, 0x74, 0x24, 0x0C,                                      // mov     esi, [esp+8+4]
        0x68, 0x00, 0x00, 0x80, 0xBF,                                // push    0BF800000h
        0x6A, 0x00,                                                  // push    0
        0xE8, (byte)(isSteam ? 0x5E : 0x6E), 0x62, 0xF5, 0xFF,                                // call    Proc_RM_LifeGet__Fv
        0xD8, 0x25, (byte)(isSteam ? 0xFC : 0x6C), (byte)(isSteam ? 0x6C : 0x6D), 0x80, 0x00,                          // fsub    dword ptr [flt_806D6C]
        0xD9, 0x1C, 0x24,                                            // fstp    dword ptr [esp]
        0xE8, (byte)(isSteam ? 0x70 : 0x80), 0x62, 0xF5, 0xFF,                                // call    Proc_RM_LifeManaSet__Fff
        0x83, 0xC4, 0x08,                                            // add     esp, 8
        0xC7, 0x86, 0x20, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40,  // mov     dword ptr [esi+620h], 40000000h
        0x5E,                                                        // pop     esi
        0x58,                                                        // pop     eax
        0xC3,                                                        // ret
    });

    private Mod_RRR_MemoryPatch raym_ETAT_paf__RemoveInstaKill => new Mod_RRR_MemoryPatch(0x00679D3F, 0x00679D2F, new byte[] {
        0x56,                               // push    esi
        0xE8, 0x7B, 0xCB, 0x00, 0x00,       // call    MG_Traire_reflex
        0x90,                               // nop
        0x8B, 0x86, 0x34, 0x06, 0x00, 0x00, // mov     eax, [esi+634h]
        0x83, 0xC4, 0x04,                   // add     esp, 4
    });

    // These do not work
    /*MemoryPatch Proc_RM_LifeMaxGet__MoreLife => new MemoryPatch(0, 0x005DCAF5, new byte[] {
        0xDB, 0x05, 0x70, 0x68, 0x80, 0x00
    });
    MemoryPatch Proc_RM_ManaMaxGet__MoreMana => new MemoryPatch(0, 0x005DCB15, new byte[] {
        0xDB, 0x05, 0x80, 0x67, 0x80, 0x00
    });
    MemoryPatch raym_ETAT_paf__RemoveInstaKills => new MemoryPatch(0, 0x00679D2F, new byte[] {
        0x68, 0x00, 0x00, 0x80, 0xBF,
        0xE8, 0xF7, 0x2D, 0xF6, 0xFF,
        0xD8, 0x25, 0xFC, 0x65, 0x80, 0x00,
        0x51,
        0xE8, 0x0B, 0x2E, 0xF6, 0xFF,
        0x83, 0xC4, 0x08,
        0xEB, 0x2B
    });*/
    /*
     * Grappin GFX:
     * 0 = Line
     * 1 = Lightning
     * 2 = Sparks
     * 3 = Corde
     * 4 = Line_Tiptop
     * */

    private int off_raymanbuffer_ptr => isSteam ? 0x0085B410 : 0x0085B430;

    private Dictionary<int, int> RMVariables_int = new Dictionary<int, int>()
    {
        [0x000011CC] = 0, // i_RM_Light
        [0x000011D0] = 1, // i_RM_Move
        [0x000011D4] = 1, // i_RM_Grappin
        [0x000011D8] = 1, // i_RM_Jump
        [0x000011DC] = 1, // i_RM_Roulade
        [0x000011E0] = 1, // i_RM_Deguisement
        [0x000011E4] = 1, // i_RM_Fight
        //[0x000009E8] = 3, // i_grappin_gfx_mode
        //[0x000011C0] = 1, // i_light_ON
        [0x00000000] = 0, // i_type_game
        //[0x000004C8] = 1, // i_move_mode
    };
    private Dictionary<int, float> RMVariables_float = new Dictionary<int, float>()
    {
        [0x000011FC] = 5f, // f_Traction_Walk,
        [0x00001200] = 200f, // f_Traction_Boost,
        [0x00002538] = 100f // f_RM_Mana
    };
    #endregion

    #region Health & Mana
    // reuse MG_Traire_ETAT_Minigame
    private Mod_RRR_MemoryPatch MG_Traire_ETAT_MiniGame__DrawFloat => new Mod_RRR_MemoryPatch(0x00685A50, 0x00685A40, isSteam ? new byte[] {
        0x53, 0x57, 0x6A, 0x00, 0x8D, 0x5C, 0x24, 0x18, 0x53, 0xBB, 0x14, 0x74, 0x80, 0x00, 0x53, 0xE8,
        0x2C, 0x2D, 0x0B, 0x00, 0x89, 0xC7, 0x83, 0xC4, 0x0C, 0x8B, 0x5C, 0x24, 0x20, 0x6A, 0x06, 0x53,
        0x57, 0xE8, 0xAA, 0x2A, 0x0B, 0x00, 0xBB, 0x74, 0x89, 0x80, 0x00, 0x53, 0x57, 0xE8, 0x2E, 0x29,
        0x0B, 0x00, 0x83, 0xC4, 0x14, 0x8B, 0x5C, 0x24, 0x24, 0x53, 0x57, 0xE8, 0x50, 0x2A, 0x0B, 0x00,
        0x68, 0xCC, 0x66, 0x80, 0x00, 0x57, 0xE8, 0x15, 0x29, 0x0B, 0x00, 0x83, 0xC4, 0x10, 0x8B, 0x5C,
        0x24, 0x0C, 0x53, 0x57, 0xE8, 0x07, 0x29, 0x0B, 0x00, 0x83, 0xC4, 0x08, 0x8B, 0x5C, 0x24, 0x10,
        0x6A, 0x02, 0x53, 0x57, 0xE8, 0x67, 0x2A, 0x0B, 0x00, 0x83, 0xC4, 0x0C, 0x5F, 0x5B, 0xC3,
    } : new byte[] {
        0x53, 0x57, 0x6A, 0x00, 0x8D, 0x5C, 0x24, 0x18, 0x53, 0xBB, 0x84, 0x74, 0x80, 0x00, 0x53, 0xE8,
        0xCC, 0x2D, 0x0B, 0x00, 0x89, 0xC7, 0x83, 0xC4, 0x0C, 0x8B, 0x5C, 0x24, 0x20, 0x6A, 0x06, 0x53,
        0x57, 0xE8, 0x4A, 0x2B, 0x0B, 0x00, 0xBB, 0xDC, 0x89, 0x80, 0x00, 0x53, 0x57, 0xE8, 0xCE, 0x29,
        0x0B, 0x00, 0x83, 0xC4, 0x14, 0x8B, 0x5C, 0x24, 0x24, 0x53, 0x57, 0xE8, 0xF0, 0x2A, 0x0B, 0x00,
        0x68, 0x3C, 0x67, 0x80, 0x00, 0x57, 0xE8, 0xB5, 0x29, 0x0B, 0x00, 0x83, 0xC4, 0x10, 0x8B, 0x5C,
        0x24, 0x0C, 0x53, 0x57, 0xE8, 0xA7, 0x29, 0x0B, 0x00, 0x83, 0xC4, 0x08, 0x8B, 0x5C, 0x24, 0x10,
        0x6A, 0x02, 0x53, 0x57, 0xE8, 0x07, 0x2B, 0x0B, 0x00, 0x83, 0xC4, 0x0C, 0x5F, 0x5B, 0xC3,
    });
    /* Arguments:
        esp+
        4    arg0 = string
        8    arg1 = float
        12    arg2.x = vector
        16        .y
        20        .z
        24    arg3 = size
        28    arg4 = color

     * Full code:
            push ebx
            push edi
            push 0
            lea ebx, [esp+12+12]
            push ebx
            mov ebx, aJyH // "\jy\\h"
            push ebx
            call    STRDATA_i_CreateText
            mov edi, eax
            add esp, 12

            mov ebx, [esp+8+24]
            push 6
            push ebx
            push edi
            call STRDATA_AppendFloat
            mov ebx, aC_1 // "\\c", found in Duel wait
            push ebx
            push edi
            call STRDATA_AppendText
            add esp, 20

            mov ebx, [esp+8+28]
            push ebx
            push edi
            call STRDATA_AppendHexa
            push asc_80673C // "\"
            push edi
            call STRDATA_AppendText
            add esp, 16

            mov ebx, [esp+8+4]
            push ebx
            push edi
            call STRDATA_AppendText
            add esp, 8

            mov ebx, [esp+8+8]
            push 2
            push ebx
            push edi
            call STRDATA_AppendFloat
            add esp, 12
            pop edi
            pop ebx
            ret
    */

    private Mod_RRR_MemoryPatch raym_track_end__CallDrawFloat1 => new Mod_RRR_MemoryPatch(0x006CC239, 0x006CC279,
        isSteam ? new byte[] { 0xE8, 0x12, 0x98, 0xFB, 0xFF }
            : new byte[] { 0xE8, 0xC2, 0x97, 0xFB, 0xFF } // call MG_Traire_ETAT_MiniGame
    );
    private Mod_RRR_MemoryPatch raym_track_end__CallDrawFloat2 => new Mod_RRR_MemoryPatch(0x006CC423, 0x006CC463,
        isSteam ? new byte[] { 0xE8, 0x28, 0x96, 0xFB, 0xFF }
            : new byte[] { 0xE8, 0xD8, 0x95, 0xFB, 0xFF }
    );
    #endregion

    #region Dance system
    private Mod_RRR_MemoryPatch MMa_Go__AddGroovyBabyToggle => new Mod_RRR_MemoryPatch(0x00490BC2, 0x00490D22, new byte[] {
        // DBG_SND
        0x6A, io_button_danse,                                                            // push    4
        0xE8, (byte)(isSteam ? 0xA7 : 0xF7), (byte)(isSteam ? 0x74 : 0x73), 0xF7, 0xFF,   // call    AI_EvalFunc_IoButtonJustPressed_C
        0x83, 0xC4, 0x04,                                                                 // add     esp, 4
        0x85, 0xC0,                                                                       // test    eax, eax
        0xBF, 0x01, 0x00, 0x00, 0x00,                                                     // mov     edi, 1
        0x74, 0x22,                                                                       // jz      short loc_490D57
        0x8B, 0x86, 0xA4, 0x00, 0x00, 0x00,                                               // mov     eax, [esi+0A4h]
        0x85, 0xC0,                                                                       // test    eax, eax
        0xC7, 0x86, 0xA4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,                       // mov     dword ptr [esi+0A4h], 0
        0x75, 0x2B,                                                                       // jnz     short loc_490D74
        0xA1, (byte)(isSteam ? 0xB4 : 0xD4), 0xB5, 0x85, 0x00,                            // mov     eax, [raymanREF_87004419]
        0x50,                                                                             // push    eax
        0xE8, (byte)(isSteam ? 0xEC : 0xCC), (byte)(isSteam ? 0xBF : 0xBE), 0x1F, 0x00,   // call    Proc_SND_Groovy_Start__FP20OBJ_tdst_GameObject_
        0x83, 0xC4, 0x04,                                                                 // add     esp, 4
        0x8B, 0x86, 0xF4, 0x00, 0x00, 0x00,                                               // mov     eax, [esi+0F4h]
        0x85, 0xC0,                                                                       // test    eax, eax
        0x0F, 0x84, 0x93, 0x03, 0x00, 0x00,                                               // jz      loc_4910F8
        0xC7, 0x86, 0xA4, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,                       // mov     dword ptr [esi+0A4h], 1
        0xE9, 0x84, 0x03, 0x00, 0x00,                                                     // jmp     loc_4910F8
        0xC7, 0x86, 0xA4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,                       // mov     dword ptr [esi+0A4h], 0
        0xE9, 0x75, 0x03, 0x00, 0x00,                                                     // jmp     loc_4910F8
    });

    private Mod_RRR_MemoryPatch Proc_RM_FullDeguisementGet__Fix1 => new Mod_RRR_MemoryPatch(0x0055271D, 0x0055283D, new byte[] { 0xEB });

    private Mod_RRR_MemoryPatch Proc_RM_FullDeguisementGet__Fix2 => new Mod_RRR_MemoryPatch(0x005526FB, 0x0055281B, new byte[] { 0xEB });

    private Mod_RRR_MemoryPatch Proc_RM_FullDeguisementGet__Fix3 => new Mod_RRR_MemoryPatch(0x005526F3, 0x00552813, new byte[] { 0x90, 0x90 });

    private Mod_RRR_MemoryPatch Proc_RM_FullDeguisementGet__Fix4 => new Mod_RRR_MemoryPatch(0x005526E9, 0x00552809, new byte[] { 0xA4, 0x7F });

    private Mod_RRR_MemoryPatch Proc_SND_Groovy_Beat__SkipTempoChecks => new Mod_RRR_MemoryPatch(0x0068CDB4, 0x0068CDF4, new byte[] { 0xEB });

    // Change beat check to use groovy flag & enable companion mode when on
    private Mod_RRR_MemoryPatch Lapin_track_reflex__ModifyBeatCheck => new Mod_RRR_MemoryPatch(0x005A8D89, 0x005A8E19, new byte[] {
        0x8B, 0x0D, (byte)(isSteam ? 0xB4 : 0xD4), 0xB5, 0x85, 0x00,    // mov     ecx, [raymanREF_87004419]
        0x51,                                  // push ecx
        0xE8, (byte)(isSteam ? 0xFB : 0xAB), 0x3D, 0x0E, 0x00,          // call    Proc_SND_Juice__FP20OBJ_tdst_GameObject_
        0x83, 0xC4, 0x04,                      // add     esp, 4
        0x85, 0xC0,                            // test    eax, eax
        0x74, 0x2E,                            // jz      short loc_5A8E5A (loc_5A8DCA on steam)
        0xBB, 0x01, 0x00, 0x00, 0x00,          // mov     ebx, 1
        0x89, 0x9E, 0xA0, 0x0A, 0x00, 0x00,    // mov     [esi+AA0h], ebx <--- sets companion mode flag
        0x89, 0x9E, 0x3C, 0x02, 0x00, 0x00,    // mov     [esi+23Ch], ebx <--- sets beat flag
        0xEB, 0x26                             // jmp     short loc_5A8E65 (loc_5A8DD5 on steam)
    });

    private const int raym_ETAT_danse__base_gog = 0x006CDEF0;
    private const int raym_ETAT_danse__base_steam = 0x006CDEB0;

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_01 => new Mod_RRR_MemoryPatch(0x006CE1A4 + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE1A4, new byte[] { (byte)(0xDD - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_02 => new Mod_RRR_MemoryPatch(0x006CE1BC + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE1BC, new byte[] { (byte)(0xDE - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_03 => new Mod_RRR_MemoryPatch(0x006CE1D4 + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE1D4, new byte[] { (byte)(0xDF - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_04 => new Mod_RRR_MemoryPatch(0x006CE1DE + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE1DE, new byte[] { (byte)(0xDC - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_05 => new Mod_RRR_MemoryPatch(0x006CE216 + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE216, new byte[] { (byte)(0xE2 - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_06 => new Mod_RRR_MemoryPatch(0x006CE22A + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE22A, new byte[] { (byte)(0xE3 - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_07 => new Mod_RRR_MemoryPatch(0x006CE253 + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE253, new byte[] { (byte)(0xE5 - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_08 => new Mod_RRR_MemoryPatch(0x006CE25A + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE25A, new byte[] { (byte)(0xE4 - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_09 => new Mod_RRR_MemoryPatch(0x006CE0BF + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE0BF, new byte[] { (byte)(0xE0 - (groovyRaymanDanceMoveset ? 20 : 0)) });

    private Mod_RRR_MemoryPatch raym_ETAT_danse__GroovyMoveset_10 => new Mod_RRR_MemoryPatch(0x006CE0C6 + raym_ETAT_danse__base_steam - raym_ETAT_danse__base_gog, 0x006CE0C6, new byte[] { (byte)(0xDC - (groovyRaymanDanceMoveset ? 20 : 0)) });

    // With LB you can set a tempo
    private Mod_RRR_MemoryPatch Proc_MM_SND_BeatManager__SetTempo_1 => new Mod_RRR_MemoryPatch(0x004692FB, 0x0046943B, new byte[] {
        0x6A, io_button_beat,
        0x31, 0xDB,
        0xE8, (byte)(isSteam ? 0x6C : 0xDC), (byte)(isSteam ? 0xE7 : 0xE6), 0xF9, 0xFF,
        0x90,
        0x90,
        0x90
    });

    private Mod_RRR_MemoryPatch Proc_MM_SND_BeatManager__SetTempo_2 => new Mod_RRR_MemoryPatch(0x0046930E, 0x0046944E, new byte[] {
        0xBB, 0x11, 0x00, 0x00, 0x00
    });
    #endregion

    #region FPS Triggers
    private Mod_RRR_MemoryPatch GST_FPS_MP_ETAT_exec__SkipTriggerCheck => new Mod_RRR_MemoryPatch(0x005CF3E7, 0x005CF3E7, new byte[] { 0xEB });

    private Mod_RRR_MemoryPatch ActivatorTrigger_wait__SkipTriggerCheck => new Mod_RRR_MemoryPatch(0x004E34BB, 0x004E364B, new byte[] { 0xEB });

    private Mod_RRR_MemoryPatch TrigTest_PivotInBV__SetGAO2ToRayman => new Mod_RRR_MemoryPatch(0x004D3A63, 0x004D3C53, new byte[] {
        0x8B, 0x35, (byte)(isSteam ? 0x24 : 0x44), 0xB7, 0x85, 0x00, // mov     esi, [raymanREF_9e00dcd3]
        0xEB, 0x19 // jmp     short loc_4D3C74
    });

    private Mod_RRR_MemoryPatch TrigTest_PivotInBV__SetGAO2ToCamera => new Mod_RRR_MemoryPatch(0x004D3A63, 0x004D3C53, new byte[] {
        0x8B, 0x35, (byte)(isSteam ? 0x8C : 0xAC), 0xB5, 0x85, 0x00, // mov     esi, [raymanREF_26005c54]
        0xEB, 0x19 // jmp     short loc_4D3C74
    });

    private Mod_RRR_MemoryPatch PNJ_Bipod_Init__DontDestroy => new Mod_RRR_MemoryPatch(0x0050D69B, 0x0050D7AB, new byte[] { 0xEB });

    private Mod_RRR_MemoryPatch PNJ_Bipod_ETAT_Basic__DontDestroy => new Mod_RRR_MemoryPatch(0x0050D7D9, 0x0050D8E9, new byte[] { 0xEB });

    private Mod_RRR_MemoryPatch PNJ_Bipod_Reflex_DontPaf => new Mod_RRR_MemoryPatch(0x0050E8D3, 0x0050E9E3, new byte[] {
        0xE9, 0x72, 0x01, 0x00, 0x00
    });

    private Mod_RRR_MemoryPatch TrigTest_PivotInBV__AlwaysTrue => new Mod_RRR_MemoryPatch(0x004D3A91, 0x004D3C81, new byte[] {
        0x90, 0x90
    });

    /*MemoryPatch TrigTest_List__ReplacePivotInBV => new MemoryPatch(0, 0x0083C7E0, addr_TrigTest_AlwaysTrue);
    MemoryPatch TrigTest_List__ReplacePivotInOneBV => new MemoryPatch(0, 0x0083C7F0, addr_TrigTest_AlwaysTrue);
    byte[] addr_TrigTest_AlwaysTrue => isSteam ? new byte[] { 0 } : new byte[] { 0x90, 0x13, 0x4D, 0x00 };
    */

    private Mod_RRR_MemoryPatch RM_MissileLauncher_Init__SetTargetToRayman => new Mod_RRR_MemoryPatch(0x0068C7FD, 0x0068C83D, new byte[] {
        0x89, 0x47, 0x40 // mov     dword ptr [edi+40h], eax
    });
    #endregion

    #region Lapin
    private Mod_RRR_MemoryPatch Lapin_track_init__Test => new Mod_RRR_MemoryPatch(0x005754B4, 0x00575544, new byte[] {
        // 0xC7, 0x86, 0x70, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Force rabbids type value (Types: 0 regular, 1 regular stunnable, 2 commander with camera focus)
        // 0xC7, 0x86, 0x44, 0x0A, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Mort_SeReleve = 1 (Rabbids go to sleep for a while when you beat them and then get up)
        // 0xC7, 0x86, 0x30, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Atk_Aspire = 1 (Rabbids use a sucking attack)
        // 0xC7, 0x86, 0x38, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Atk_Souffle = 1 (Rabbids use a blowing attack)
        // 0xC7, 0x86, 0x40, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Atk_Taupe = 1 (Rabbids will dig into the ground and speed toward you, then spin out from underneath)
        // 0xC7, 0x86, 0x50, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Atk_Frapper_et_reculer = 1 (Rabbids will hit and move back & laugh at you)
        // 0xC7, 0x86, 0x7C, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Atk_Grappin_Controle = 1 (Rabbids will freeze when grappined since the grappin controle state is missing)
        // 0xC7, 0x86, 0x84, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Atk_Anti_Mashing = 1 (Rabbids will survive combos unless their life is 0 after the first hit)
        // 0xC7, 0x86, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC8, 0x42 // Set Rabbids Life_Max value to 100
        // 0xC7, 0x86, 0x98, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 // Set Bonus_lum to 1 (does nothing)
        // 0xC7, 0x86, 0x90, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00 // Spawn 5 bonus life (crash)
        // 0xC7, 0x86, 0x94, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00 // Spawn 5 bonus mana (crash)
    });

    private Mod_RRR_MemoryPatch Proc_PNJ_Lapin_DANSE_Init__RandomProtoPowers => new Mod_RRR_MemoryPatch(0x005736ED, 0x0057377D, new byte[] {
        0x6A, 0x0A,                                                   // push    0Ah    // increase for lower chance
        0x6A, 0x00,                                                   // push    0
        0xE8, (byte)(isSteam ? 0xEA : 0x0A), (byte)(isSteam ? 0xE9 : 0xEA), 0xEA, 0xFF,                                 // call    AI_EvalFunc_MATHRandInt_C
        0x83, 0xC4, 0x08,                                             // add     esp, 8
        0x85, 0xC0,                                                   // test    eax, eax
        0x74, 0x14,                                                   // jz      [give power Aspire]
        0x48,                                                         // dec		eax
        0x85, 0xC0,                                                   // test	eax, eax
        0x74, 0x1E,                                                   // jz		[give power Soufle]
        0x48,                                                         // dec		eax
        0x85, 0xC0,                                                   // test	eax, eax
        0x74, 0x28,                                                   // jz		[give power Taupe]
        0x48,                                                         // dec		eax
        0x85, 0xC0,                                                   // test	eax, eax
        0x74, 0x32,                                                   // jz		[give power Mort_SeReleve]
        0xE9, 0xB7, 0x01, 0x00, 0x00,                                 // jmp		[end]
        0xC7, 0x86, 0x30, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,   // mov     dword ptr [esi+0730h], 1 //give power aspire
        0xE9, 0xA8, 0x01, 0x00, 0x00,                                 // jmp		[end]
        0xC7, 0x86, 0x38, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,   // mov     dword ptr [esi+0738h], 1 //give power souffle
        0xE9, 0x99, 0x01, 0x00, 0x00,                                 // jmp		[end]
        0xC7, 0x86, 0x40, 0x07, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,   // mov     dword ptr [esi+0740h], 1 //give power taupe
        0xE9, 0x8A, 0x01, 0x00, 0x00,                                 // jmp		[end]
        0xC7, 0x86, 0x44, 0x0A, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,   // mov     dword ptr [esi+0A44h], 1 //give power Mort_SeReleve
        0xE9, 0x7B, 0x01, 0x00, 0x00                                  // jmp		[end]
    });

    private Mod_RRR_MemoryPatch Lapin_track_init__RandomProtoPowers1 => new Mod_RRR_MemoryPatch(0x00576E6A, 0x00576EFA, new byte[] {
        0x90, 0x90
    });

    private Mod_RRR_MemoryPatch Lapin_track_init__RandomProtoPowers2 => new Mod_RRR_MemoryPatch(0x00576E81, 0x00576F11, new byte[] {
        0x90, 0x90,0x90, 0x90,0x90
    });

    private Mod_RRR_MemoryPatch Lapin_track_init__IncreaseMaxLife => new Mod_RRR_MemoryPatch(0x005754B4, 0x00575544, new byte[] {
        0xC7, 0x86, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC8, 0x42 // Set Rabbids Life_Max value to 100
    });

    // Make rabbids drop their items
    private Mod_RRR_MemoryPatch Lapin_ITEM_Destroy__DropItems => new Mod_RRR_MemoryPatch(0x0049B551, 0x0049B731, new byte[] { 0xEB }); // jmp
    #endregion

    #region Mounts
    // Force rabbids off mounts
    private Mod_RRR_MemoryPatch Lapin_track_init__ForceOffMount => new Mod_RRR_MemoryPatch(0x005761DF, 0x0057626F, new byte[] {
        0xB8, 0x00, 0x00, 0x00, 0x00, // mov eax, 0
        0x90                          // nop
    });

    // Allows jumping off all mounts
    private Mod_RRR_MemoryPatch RM_Mount_Jump_OFF__RemoveIDCheck => new Mod_RRR_MemoryPatch(0x004ED84D, 0x004ED9DD, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 // a lotta nops
    });

    // PNJ_Spider (most functions here have the same address for Steam & GOG)
    private Mod_RRR_MemoryPatch PNJ_Spider_Reflex__RemoveNextWP => new Mod_RRR_MemoryPatch(0x005E569F, 0x005E569F, new byte[] {
        0x8B, 0x86, 0x24, 0x02, 0x00, 0x00,        // mov     eax, [esi+224h]
        0x85, 0xC0,                                // test    eax, eax
        0x57,                                      // push    edi
        0x74, 0x09,                                // jz      short loc_5E56B3
        0xC7, 0x46, 0x10, 0x00, 0x00, 0x00, 0x00,  // mov     dword ptr [esi+10h], 0
        0x90,                                      // nop
        0x90,                                      // nop
    });

    private Mod_RRR_MemoryPatch PNJ_Spider_Init__UseSpiderType => new Mod_RRR_MemoryPatch(0x005E497E, 0x005E497E, new byte[] {
        0xC7, 0x07, 0x01, 0x00, 0x00, 0x00,
        0x90
    });

    private Mod_RRR_MemoryPatch PNJ_Spider_Init_IK__Use4LegsForSpiderType => new Mod_RRR_MemoryPatch(0x005C8B74, 0x005C8B74, new byte[] { 0x04 });

    private Mod_RRR_MemoryPatch PNJ_Spider_RidedPosGet__FixGaoCanalForSpider => new Mod_RRR_MemoryPatch(0x004FAEA1, 0x004FB021, new byte[] { 0xEB, 0x1E });

    private Mod_RRR_MemoryPatch PNJ_Spider_ETAT_Basic__AllowJumpForQuadri => new Mod_RRR_MemoryPatch(0x00606A99, 0x00606A89, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90,0x90 // Nop the creature type check
    });

    private Mod_RRR_MemoryPatch PNJ_Spider_Init__HalfSizeForSpiderType => new Mod_RRR_MemoryPatch(0x005E4A67, 0x005E4A67, new byte[] {
        0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90,0x90
    });

    // PNJ_Prune
    private Mod_RRR_MemoryPatch PNJ_Prune_init__UsePlumTypeForPig_Type0 => new Mod_RRR_MemoryPatch(0x0046348E, 0x0046354E, new byte[] {
        0xC7, 0x06, 0x00, 0x00, 0x00, 0x00, // mov     dword ptr [esi], 0
        0xE9, 0x4C, 0xFD, 0xFF, 0xFF
    });

    private Mod_RRR_MemoryPatch PNJ_Prune_init__UsePlumTypeForPig_Type1 => new Mod_RRR_MemoryPatch(0x0046348E, 0x0046354E, new byte[] {
        0xC7, 0x06, 0x01, 0x00, 0x00, 0x00, // mov     dword ptr [esi], 1
        0xE9, 0x0A, 0xFE, 0xFF, 0xFF
    });

    private Mod_RRR_MemoryPatch PNJ_Prune_init__UsePlumTypeForPig_Type16 => new Mod_RRR_MemoryPatch(0x0046348E, 0x0046354E, new byte[] {
        0xC7, 0x06, 0x10, 0x00, 0x00, 0x00, // mov     dword ptr [esi], 1
        0xE9, 0xB9, 0xFE, 0xFF, 0xFF
    });


    // PNJ_Volant
    private Mod_RRR_MemoryPatch PNJ_Volant_Init__StartOutOfFPSMode_Parked => new Mod_RRR_MemoryPatch(0x004E42A2, 0x004E4432, new byte[] { 0xEB }); // jmp

    private Mod_RRR_MemoryPatch PNJ_Volant_Init__StartOutOfFPSMode_Flying => new Mod_RRR_MemoryPatch(0x004E42A4, 0x004E4434, new byte[] {
        0xA1, (byte)(isSteam ? 0x5C : 0x7C), 0xA3, 0x85, 0x00
    });

    private Mod_RRR_MemoryPatch PNJ_Volant_init__UseEagleTypeForBat => new Mod_RRR_MemoryPatch(0x004E3805, 0x004E3995, new byte[] {
        0xC7, 0x06, 0x00, 0x00, 0x00, 0x00, // mov     dword ptr [esi], 0
        0x90,                               // nop
        0x90,                               // nop
        0x90                                // nop
    });

    // Skip bat's code forcing Rayman's position to its own
    // Note: the bat shoot code changes that part of the code to the eagle's.
    // The bat's projectile is null in the minigame, but the eagle's works.
    private Mod_RRR_MemoryPatch PNJ_Volant_callback_afterblend__RemoveRaymanPositionChange => new Mod_RRR_MemoryPatch(0x004E9A92, 0x004E9C22, new byte[] {
        0xE9, (byte)(makeBatShoot ? 0xC4 : 0xAF), 0x00, 0x00, 0x00
    });

    private Mod_RRR_MemoryPatch PNJ_Volant_Shoot__MakeSoucoupeShoot => new Mod_RRR_MemoryPatch(0x004FFF68, 0x005000E8, new byte[] {
        0xEB                          // jmp ...
    });

    // Make PNJ_Quadri agressive
    private Mod_RRR_MemoryPatch PNJ_Quadri_Init__MakeAgressive => new Mod_RRR_MemoryPatch(0x004FB403, 0x004FB583, new byte[] {
        0xB8, 0x01, 0x00, 0x00, 0x00, // mov     eax, 1
        0x89, 0x46, 0x0C,             // mov     [esi+0Ch], eax
        0xB8, 0x00, 0x00, 0x00, 0x00, // mov     eax, 0
        0x90,                         // nop
        0xEB                          // jmp ...
    });
    #endregion

    #region Minigame
    private Mod_RRR_MemoryPatch GST_MiniGame_Init__DeactivateIntro_1 => new Mod_RRR_MemoryPatch(0x0068E2C9, 0x0068E309, new byte[] {
        0xC7, 0x46, 0x08, 0x00, 0x00, 0x00, 0x00,
        0xEB, 0x16
    });

    private Mod_RRR_MemoryPatch GST_MiniGame_Player_ShapeSet__SetPlayerCostume => new Mod_RRR_MemoryPatch(0x00677BAC, 0x00677B9C, new byte[] {
        0x8B, 0x1D, (byte)(isSteam ? 0x68 : 0x88), 0x9D, 0x85, 0x00,
        0x52,
        0x53,
        0xE8, 0xC7, 0x25, 0xFC, 0xFF,
        0x83, 0xC4, 0x08,
        0xC3,
    });
    #endregion

    #region Sounds
    // Disable footstep error sound. Also disables regular footstep sounds in the prison & arena.
    private Mod_RRR_MemoryPatch raym_exec_SND__DisableFootstep => new Mod_RRR_MemoryPatch(0x0068047D, 0x0068046D, new byte[] { 0x00 });

    private Mod_RRR_MemoryPatch SB_Init__MergeAllSoundbanksWithRaymans => new Mod_RRR_MemoryPatch(0x00490B38, 0x00490C98, new byte[] {
        0x56,                                  // push    esi
        0x8B, 0x35, (byte)(isSteam ? 0xAC : 0xCC), 0x9A, 0x85, 0x00,    // mov     esi, [AI_gpst_CurrentGameObject]
        0xA1, (byte)(isSteam ? 0x68 : 0x88), 0xAF, 0x85, 0x00,          // mov     eax, [_COMMON_LIBREF_9e00dcd3]
        0x90                                   // nop
    });

    private Mod_RRR_MemoryPatch GS_exec_SetMicro__SkipPresetMicroCheck => new Mod_RRR_MemoryPatch(0x0046C478, 0x0046C5B8, new byte[] {
        0xB8, 0x00, 0x00, 0x00, 0x00, // mov eax, 0
        0x90                          // nop
    });
    #endregion

    #region Menus
    private Mod_RRR_MemoryPatch IntMIG_Page_MapList__ChangeBinCheck => new Mod_RRR_MemoryPatch(0x00664055, 0x00664045, new byte[] {
        0x74 // jz
    });

    /*MemoryPatch IntMIG_AFaireToutLeTempsAUDEBUT__AddCheatCheck => new MemoryPatch(0, 0x0066D4E6, new byte[] {
        0xE8, 0xE5, 0x74, 0xE6, 0xFF,
        0xEB, 0x17
    });*/
    #endregion

    #region Button config
    private byte io_button_noclip => 8; // Back

    private byte io_button_danse => 12; // DPad Up

    private byte io_button_beat => 4; // LB

    private byte io_button_finisher => 5; // RB
    /*
     * Buttons:
     * 4 = LB
     * 5 = RB
     * 6 = LT
     * 7 = RT
     * 8 = Back
     * 9 = Start
     * 10 = ?
     * 11 = ?
     * 12 = DPad Up
     * 13 = DPad Right
     * 14 = DPad Down
     * */
    #endregion

    #region AI2C_fctDefs
    // AE_Init destroys the GameObject if it doesn't have the Event flag. Very useful for disabling minigame functions

    private int off_AE_Init => isSteam ? off_AE_Init_Steam : off_AE_Init_GOG;

    private const int off_AE_Init_Steam = 0x004C7B20;
    private const int off_AE_Init_GOG = 0x004C7D10;

    private int off_AI2C_fctDefs => 0x00834050; // Same for GOG & Steam versions
    #endregion

    #region Keyboard controls
    /*MemoryPatch SetWorldKeyMapping__FixKeyboardControls1 => new MemoryPatch(0, 0x007C3140, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90
    });
    MemoryPatch SetWorldKeyMapping__FixKeyboardControls2 => new MemoryPatch(0, 0x007C3179, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90
    });*/
    private Mod_RRR_MemoryPatch SetWorldKeyMapping__FixKeyboardControls1 => new Mod_RRR_MemoryPatch(0x007C3070, 0x007C3140, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90,
        0xC7, 0x05, (byte)(isSteam ? 0x4C : 0x74), 0x71, 0xC8, 0x00, (byte)(isSteam ? 0xA0 : 0x70), (byte)(isSteam ? 0x28 : 0x29), 0x7C, 0x00
    });
    private Mod_RRR_MemoryPatch SetWorldKeyMapping__FixKeyboardControls2 => new Mod_RRR_MemoryPatch(0x007C30A9, 0x007C3179, new byte[] {
        0x90, 0x90, 0x90, 0x90, 0x90,
        0xC7, 0x05, (byte)(isSteam ? 0x4C : 0x74), 0x71, 0xC8, 0x00, (byte)(isSteam ? 0xA0 : 0x70), (byte)(isSteam ? 0x28 : 0x29), 0x7C, 0x00
    });
    private Mod_RRR_MemoryPatch SetWorldKeyMapping__FixKeyboardControls3 => new Mod_RRR_MemoryPatch(0x007C3021, 0x007C30F1, new byte[] {
        0xEB
    });
    private Mod_RRR_MemoryPatch CheckControlMode_Gladiator__FixKeyboardControls4 => new Mod_RRR_MemoryPatch(0x007C2AFD, 0x007C2BCD, new byte[] {
        0xD8, 0x0D, (byte)(isSteam ? 0x3C : 0xAC), 0x66, 0x80, 0x00
    });
    private int addr_KeyboardKeycodes => isSteam ? 0x00856F98 : 0x00856FA8;
    public int[] KeyboardKeycodes { get; } = new int[] 
    {
        0x1E, // 00. (A) Jump               - Default: A
        0x1C, // 01. (A)                    - Default: Enter
        0x0E, // 02. (B) Dance mode: turn   - Default: Backspace
        0,    // 03. (B)
        0x39, // 04. (X) Attack             - Default: Space
        0,    // 05. (X)
        0x1F, // 06. (Y) Grapple hook       - Default key: S
        0,    // 07. (Y)
        0x1D, // 08. (LB) Light/Tempo       - Default key: LCtrl
        0,    // 09. (LB)
        0x2D, // 10. (RB) Attack (Finisher) - Default key: X
        0,    // 11. (RB)
        0x12, // 12. (LT) Look mode         - Default key: E
        0,    // 13. (LT)        
        0x2C, // 14. (RT) Roll/Ground pound - Default key: W
        0,    // 15. (RT)
        0x19, // 16. (Back) Noclip          - Default key: P
        0,    // 17. (Back)
        0x01, // 18. (Start) Pause          - Default key: Esc
        0,    // 19. (Start)
        0x25, // 20. (???)                  - Default key: K
        0,    // 21. (???)          
        0x24, // 22. (???)                  - Default key: J
        0,    // 23. (???)            
        0x20, // 24. (DPad Up) Dance Toggle - Default key: D
        0,    // 25. (DPad Up)
        0x32, // 26. (???)                  - Default key: ,
        0,    // 27. (???)
        0x2A, // 28. (Analog modifier) Walk - Default key: left shift
        0,    // 29. (Analog modifier)
        0,    // 30. (???)
        0,    // 31. (???)
        0,    // 32. Open Menu (same as Start)
        0,    // 33. Open Menu (same as Start)
    };
    #endregion
}