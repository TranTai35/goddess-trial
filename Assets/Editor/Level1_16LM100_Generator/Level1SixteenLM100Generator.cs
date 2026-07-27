#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class Level1SixteenLM100Generator
{
    private const string BasePath =
        "Assets/MapAndCharacter/Packed_RPGTinyHeroWorldBundlePBR/" +
        "RPG Tiny Fantasy World 01 PBR/Prefab/";

    private const string OutputScene =
        "Assets/Scenes/Level1_16LM100_BranchArenas.unity";

    private const float RoadOriginalLength = 13.99f;
    private const float Bridge02OriginalLength = 24.267f;

    private static readonly Vector3 PlayerSpawn = new Vector3(150f, 0.45f, -25f);
    private static readonly Vector3 SmallArenaCenter = new Vector3(150f, 0.05f, 105f);
    private static readonly Vector3 ForkCenter = new Vector3(150f, 0.05f, 165f);
    private static readonly Vector3 LeftArenaCenter = new Vector3(0f, 0.05f, 200f);
    private static readonly Vector3 RightArenaCenter = new Vector3(300f, 0.05f, 200f);
    private static readonly Vector3 TempleCenter = new Vector3(150f, 0.05f, 400f);

    private static readonly List<Vector3> MainPath = new List<Vector3>
    {
        new Vector3(150f, 0.08f, -36f),
        new Vector3(150f, 0.08f, 45f),
        new Vector3(150f, 0.08f, 82f),
        new Vector3(150f, 0.08f, 128f),
        new Vector3(150f, 0.08f, 165f)
    };

    private static readonly List<Vector3> LeftBranchPath = new List<Vector3>
    {
        new Vector3(150f, 0.08f, 165f),
        new Vector3(118f, 0.08f, 178f),
        new Vector3(82f, 0.08f, 192f),
        new Vector3(43f, 0.08f, 200f),
        new Vector3(0f, 0.08f, 200f)
    };

    private static readonly List<Vector3> RightBranchPath = new List<Vector3>
    {
        new Vector3(150f, 0.08f, 165f),
        new Vector3(182f, 0.08f, 178f),
        new Vector3(218f, 0.08f, 192f),
        new Vector3(257f, 0.08f, 200f),
        new Vector3(300f, 0.08f, 200f)
    };

    [MenuItem("Tools/Goddess Trial/Generate Level1 - 16 LM100 Branch Arenas")]
    public static void Generate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureOutputFolder();

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        GameObject root = NewRoot("LEVEL1_16_LM100_BRANCH_ARENAS");
        GameObject environment = NewChild(root, "Environment");
        GameObject gameplay = NewChild(root, "Gameplay_Markers");
        GameObject lighting = NewChild(root, "Lighting");

        GameObject ocean = NewChild(environment, "00_Ocean");
        GameObject land = NewChild(environment, "01_LM100_Grid_Exact_Positions");
        GameObject roads = NewChild(environment, "02_Roads");
        GameObject bridges = NewChild(environment, "03_Bridges");
        GameObject trees = NewChild(environment, "04_Trees");
        GameObject rocks = NewChild(environment, "05_Rocks");
        GameObject mountains = NewChild(environment, "06_Mountains");
        GameObject lakes = NewChild(environment, "07_Lakes");
        GameObject plants = NewChild(environment, "08_Grass_And_Flowers");
        GameObject structures = NewChild(environment, "09_Structures_And_Ruins");
        GameObject effects = NewChild(environment, "10_Fire_Crystals_And_Details");

        BuildOcean(ocean.transform);
        BuildExactSixteenLandMasses(land.transform);
        BuildRoadNetwork(roads.transform);
        BuildLongBridges(bridges.transform, structures.transform);

        BuildStartArea(structures.transform, effects.transform);
        BuildSmallEncounter(structures.transform, rocks.transform, effects.transform);
        BuildForkArea(structures.transform, effects.transform);
        BuildLeftLargeArena(structures.transform, rocks.transform, effects.transform);
        BuildRightLargeArena(structures.transform, rocks.transform, effects.transform);
        BuildLakes(lakes.transform, rocks.transform, plants.transform);
        BuildUpperTemple(structures.transform, effects.transform, rocks.transform);

        BuildMountainBorder(mountains.transform, rocks.transform);
        ScatterForestAndDetails(trees.transform, rocks.transform, plants.transform);
        BuildGameplayMarkers(gameplay.transform);
        BuildLightingAndAtmosphere(lighting.transform);

        EditorSceneManager.SaveScene(scene, OutputScene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(OutputScene);
        Selection.activeObject = sceneAsset;
        EditorGUIUtility.PingObject(sceneAsset);

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.pivot = new Vector3(150f, 0f, 190f);
            sceneView.rotation = Quaternion.Euler(52f, -28f, 0f);
            sceneView.size = 260f;
            sceneView.Repaint();
        }

        EditorUtility.DisplayDialog(
            "Level 1 created",
            "Created:\n" + OutputScene +
            "\n\nThe scene contains exactly 16 LM100 prefabs at the requested positions, " +
            "a player start area, one 4-enemy encounter, left and right 15-enemy arenas, " +
            "two long bridges, lakes, roads, trees, mountains and an upper temple.\n\n" +
            "Replace the empty spawn markers with your gameplay prefabs and bake NavMesh.",
            "OK");
    }

    private static void BuildOcean(Transform parent)
    {
        Place(
            "LandMass/Ocean.prefab",
            parent,
            "Ocean_Under_Whole_Level",
            new Vector3(150f, -48f, 190f),
            Quaternion.identity,
            new Vector3(1.35f, 1f, 1.35f));
    }

    private static void BuildExactSixteenLandMasses(Transform parent)
    {
        float[] xPositions = { 0f, 100f, 200f, 300f };
        float[] zPositions = { 400f, 200f, 100f, 0f };

        int index = 1;
        for (int row = 0; row < zPositions.Length; row++)
        {
            for (int column = 0; column < xPositions.Length; column++)
            {
                Vector3 position = new Vector3(
                    xPositions[column],
                    0f,
                    zPositions[row]);

                Place(
                    "LandMass/LM100.prefab",
                    parent,
                    "LM100_" + index.ToString("00") +
                    "_X" + xPositions[column].ToString("0") +
                    "_Z" + zPositions[row].ToString("0"),
                    position,
                    Quaternion.identity,
                    Vector3.one);

                index++;
            }
        }
    }

    private static void BuildRoadNetwork(Transform parent)
    {
        PlaceRoadPolyline(parent, "Main_Road", MainPath,
            "RiverRoadLakeFall/RoadA01.prefab", 1.18f);

        PlaceRoadPolyline(parent, "Left_Branch_Road", LeftBranchPath,
            "RiverRoadLakeFall/RoadC01.prefab", 1.22f);

        PlaceRoadPolyline(parent, "Right_Branch_Road", RightBranchPath,
            "RiverRoadLakeFall/RoadC02.prefab", 1.22f);

        PlaceRoadLine(parent, "Left_Arena_To_Bridge",
            new Vector3(0f, 0.08f, 225f),
            new Vector3(100f, 0.08f, 248f),
            "RiverRoadLakeFall/RoadB01.prefab", 1.08f);

        PlaceRoadLine(parent, "Right_Arena_To_Bridge",
            new Vector3(300f, 0.08f, 225f),
            new Vector3(200f, 0.08f, 248f),
            "RiverRoadLakeFall/RoadB02.prefab", 1.08f);

        PlaceRoadLine(parent, "Upper_Left_Road",
            new Vector3(100f, 0.08f, 352f),
            new Vector3(145f, 0.08f, 398f),
            "RiverRoadLakeFall/RoadD01.prefab", 1.12f);

        PlaceRoadLine(parent, "Upper_Right_Road",
            new Vector3(200f, 0.08f, 352f),
            new Vector3(155f, 0.08f, 398f),
            "RiverRoadLakeFall/RoadD02.prefab", 1.12f);
    }

    private static void BuildLongBridges(Transform parent, Transform structures)
    {
        const float bridgeLength = 104f;
        float zScale = bridgeLength / Bridge02OriginalLength;

        Place(
            "BuildingUtilityDeco/Bridge02.prefab",
            parent,
            "Long_Bridge_Left_X100",
            new Vector3(100f, 0.32f, 300f),
            Quaternion.identity,
            new Vector3(1.25f, 1f, zScale));

        Place(
            "BuildingUtilityDeco/Bridge02.prefab",
            parent,
            "Long_Bridge_Right_X200",
            new Vector3(200f, 0.32f, 300f),
            Quaternion.identity,
            new Vector3(1.25f, 1f, zScale));

        Vector3[] gatePositions =
        {
            new Vector3(100f, 0.16f, 248f),
            new Vector3(200f, 0.16f, 248f),
            new Vector3(100f, 0.16f, 352f),
            new Vector3(200f, 0.16f, 352f)
        };

        for (int i = 0; i < gatePositions.Length; i++)
        {
            Place(
                "BuildingUtilityDeco/Pillar02.prefab",
                structures,
                "Bridge_Pillar_A_" + (i + 1).ToString("00"),
                gatePositions[i] + new Vector3(-5f, 0f, 0f),
                Quaternion.identity,
                Vector3.one * 1.15f);

            Place(
                "BuildingUtilityDeco/Pillar02.prefab",
                structures,
                "Bridge_Pillar_B_" + (i + 1).ToString("00"),
                gatePositions[i] + new Vector3(5f, 0f, 0f),
                Quaternion.identity,
                Vector3.one * 1.15f);
        }
    }

    private static void BuildStartArea(Transform structures, Transform effects)
    {
        Place(
            "BuildingUtilityDeco/Portal01.prefab",
            structures,
            "Start_Portal",
            new Vector3(150f, 0.18f, -39f),
            Quaternion.identity,
            Vector3.one * 1.25f);

        Place(
            "BuildingUtilityDeco/Gate01.prefab",
            structures,
            "Start_Gate",
            new Vector3(150f, 0.12f, -30f),
            Quaternion.identity,
            new Vector3(1.35f, 1.2f, 1.35f));

        Place(
            "BuildingUtilityDeco/SignPost01.prefab",
            structures,
            "Start_Sign",
            new Vector3(136f, 0.1f, -10f),
            Quaternion.Euler(0f, 22f, 0f),
            Vector3.one * 1.1f);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Start_Campfire",
            new Vector3(167f, 0.1f, -12f),
            Quaternion.identity,
            Vector3.one);

        Place(
            "BuildingUtilityDeco/WoodLog01.prefab",
            structures,
            "Start_Log_A",
            new Vector3(171f, 0.08f, -7f),
            Quaternion.Euler(0f, 25f, 0f),
            Vector3.one);

        Place(
            "BuildingUtilityDeco/WoodLog02.prefab",
            structures,
            "Start_Log_B",
            new Vector3(164f, 0.08f, -3f),
            Quaternion.Euler(0f, -18f, 0f),
            Vector3.one);
    }

    private static void BuildSmallEncounter(
        Transform structures,
        Transform rocks,
        Transform effects)
    {
        Place(
            "BuildingUtilityDeco/Gate02.prefab",
            structures,
            "Small_Encounter_Entrance",
            new Vector3(150f, 0.12f, 73f),
            Quaternion.identity,
            new Vector3(1.25f, 1.1f, 1.25f));

        Place(
            "Rock/RockCliff02.prefab",
            rocks,
            "Small_Arena_Left_Rock",
            new Vector3(122f, 0.12f, 107f),
            Quaternion.Euler(0f, 42f, 0f),
            Vector3.one * 1.3f);

        Place(
            "Rock/RockCliff04.prefab",
            rocks,
            "Small_Arena_Right_Rock",
            new Vector3(178f, 0.12f, 104f),
            Quaternion.Euler(0f, -38f, 0f),
            Vector3.one * 1.25f);

        Place(
            "BuildingUtilityDeco/WatchTower01.prefab",
            structures,
            "Small_Arena_WatchTower",
            new Vector3(182f, 0.1f, 128f),
            Quaternion.Euler(0f, -135f, 0f),
            Vector3.one * 1.05f);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Small_Arena_Fire",
            new Vector3(125f, 0.1f, 128f),
            Quaternion.identity,
            Vector3.one);

        PlaceCobbleRing(rocks, SmallArenaCenter, 29f, 12, "Small_Arena_Cobble");
    }

    private static void BuildForkArea(Transform structures, Transform effects)
    {
        Place(
            "BuildingUtilityDeco/SignPost06.prefab",
            structures,
            "Fork_Direction_Sign",
            ForkCenter + new Vector3(0f, 0.08f, 0f),
            Quaternion.Euler(0f, 8f, 0f),
            Vector3.one * 1.3f);

        Place(
            "BuildingUtilityDeco/Pillar01.prefab",
            structures,
            "Fork_Pillar_Left",
            new Vector3(132f, 0.1f, 169f),
            Quaternion.identity,
            Vector3.one * 1.1f);

        Place(
            "BuildingUtilityDeco/Pillar01.prefab",
            structures,
            "Fork_Pillar_Right",
            new Vector3(168f, 0.1f, 169f),
            Quaternion.identity,
            Vector3.one * 1.1f);

        Place(
            "BuildingUtilityDeco/Crystal01.prefab",
            effects,
            "Fork_Crystal_Left",
            new Vector3(127f, 0.15f, 162f),
            Quaternion.identity,
            Vector3.one * 0.8f);

        Place(
            "BuildingUtilityDeco/Crystal01.prefab",
            effects,
            "Fork_Crystal_Right",
            new Vector3(173f, 0.15f, 162f),
            Quaternion.identity,
            Vector3.one * 0.8f);
    }

    private static void BuildLeftLargeArena(
        Transform structures,
        Transform rocks,
        Transform effects)
    {
        Place(
            "BuildingUtilityDeco/Gate03.prefab",
            structures,
            "Left_Arena_Entrance_Gate",
            LeftArenaCenter + new Vector3(43f, 0.12f, 0f),
            Quaternion.Euler(0f, 90f, 0f),
            new Vector3(1.35f, 1.15f, 1.35f));

        Place(
            "BuildingUtilityDeco/WatchTower01.prefab",
            structures,
            "Left_Arena_WatchTower_NW",
            LeftArenaCenter + new Vector3(-34f, 0.1f, 33f),
            Quaternion.Euler(0f, 45f, 0f),
            Vector3.one * 1.15f);

        Place(
            "BuildingUtilityDeco/WatchTower01.prefab",
            structures,
            "Left_Arena_WatchTower_SW",
            LeftArenaCenter + new Vector3(-34f, 0.1f, -33f),
            Quaternion.Euler(0f, 135f, 0f),
            Vector3.one * 1.15f);

        Place(
            "Rock/Rock06.prefab",
            rocks,
            "Left_Arena_Cover_A",
            LeftArenaCenter + new Vector3(-14f, 0.12f, 7f),
            Quaternion.Euler(0f, 25f, 0f),
            Vector3.one * 1.35f);

        Place(
            "Rock/Rock09.prefab",
            rocks,
            "Left_Arena_Cover_B",
            LeftArenaCenter + new Vector3(17f, 0.12f, -12f),
            Quaternion.Euler(0f, 80f, 0f),
            Vector3.one * 1.25f);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Left_Arena_Fire_North",
            LeftArenaCenter + new Vector3(0f, 0.1f, 39f),
            Quaternion.identity,
            Vector3.one);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Left_Arena_Fire_South",
            LeftArenaCenter + new Vector3(0f, 0.1f, -39f),
            Quaternion.identity,
            Vector3.one);

        PlaceArenaPerimeter(
            LeftArenaCenter,
            structures,
            rocks,
            true,
            "Left_Arena");
    }

    private static void BuildRightLargeArena(
        Transform structures,
        Transform rocks,
        Transform effects)
    {
        Place(
            "BuildingUtilityDeco/Gate04.prefab",
            structures,
            "Right_Arena_Entrance_Gate",
            RightArenaCenter + new Vector3(-43f, 0.12f, 0f),
            Quaternion.Euler(0f, -90f, 0f),
            new Vector3(1.35f, 1.15f, 1.35f));

        Place(
            "BuildingUtilityDeco/WatchTower01.prefab",
            structures,
            "Right_Arena_WatchTower_NE",
            RightArenaCenter + new Vector3(34f, 0.1f, 33f),
            Quaternion.Euler(0f, -45f, 0f),
            Vector3.one * 1.15f);

        Place(
            "BuildingUtilityDeco/WatchTower01.prefab",
            structures,
            "Right_Arena_WatchTower_SE",
            RightArenaCenter + new Vector3(34f, 0.1f, -33f),
            Quaternion.Euler(0f, -135f, 0f),
            Vector3.one * 1.15f);

        Place(
            "Rock/Rock04.prefab",
            rocks,
            "Right_Arena_Cover_A",
            RightArenaCenter + new Vector3(14f, 0.12f, 8f),
            Quaternion.Euler(0f, 60f, 0f),
            Vector3.one * 1.35f);

        Place(
            "Rock/Rock08.prefab",
            rocks,
            "Right_Arena_Cover_B",
            RightArenaCenter + new Vector3(-17f, 0.12f, -11f),
            Quaternion.Euler(0f, 15f, 0f),
            Vector3.one * 1.25f);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Right_Arena_Fire_North",
            RightArenaCenter + new Vector3(0f, 0.1f, 39f),
            Quaternion.identity,
            Vector3.one);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Right_Arena_Fire_South",
            RightArenaCenter + new Vector3(0f, 0.1f, -39f),
            Quaternion.identity,
            Vector3.one);

        PlaceArenaPerimeter(
            RightArenaCenter,
            structures,
            rocks,
            false,
            "Right_Arena");
    }

    private static void PlaceArenaPerimeter(
        Vector3 center,
        Transform structures,
        Transform rocks,
        bool openingOnRight,
        string prefix)
    {
        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f;
            float entranceAngle = openingOnRight ? 0f : 180f;

            if (Mathf.Abs(Mathf.DeltaAngle(angle, entranceAngle)) < 25f)
                continue;

            float radians = angle * Mathf.Deg2Rad;
            Vector3 position = center + new Vector3(
                Mathf.Cos(radians) * 44f,
                0.08f,
                Mathf.Sin(radians) * 44f);

            if (i % 3 == 0)
            {
                Place(
                    "Rock/RockCliff0" + ((i % 6) + 1) + ".prefab",
                    rocks,
                    prefix + "_Perimeter_Rock_" + (i + 1).ToString("00"),
                    position,
                    Quaternion.Euler(0f, -angle + 90f, 0f),
                    Vector3.one * 1.1f);
            }
            else
            {
                Place(
                    "BuildingUtilityDeco/WoodFence0" + ((i % 5) + 1) + ".prefab",
                    structures,
                    prefix + "_Perimeter_Fence_" + (i + 1).ToString("00"),
                    position,
                    Quaternion.Euler(0f, -angle + 90f, 0f),
                    new Vector3(1.25f, 1f, 1.25f));
            }
        }
    }

    private static void BuildLakes(
        Transform lakes,
        Transform rocks,
        Transform plants)
    {
        Place(
            "RiverRoadLakeFall/Lake02.prefab",
            lakes,
            "Lower_Left_Lake",
            new Vector3(32f, 0.12f, 92f),
            Quaternion.Euler(0f, 18f, 0f),
            new Vector3(1.05f, 1f, 1.05f));

        Place(
            "RiverRoadLakeFall/Lake03.prefab",
            lakes,
            "Lower_Right_Lake",
            new Vector3(268f, 0.12f, 93f),
            Quaternion.Euler(0f, -24f, 0f),
            new Vector3(1.02f, 1f, 1.02f));

        Place(
            "RiverRoadLakeFall/Lake01.prefab",
            lakes,
            "Upper_Left_Lake",
            new Vector3(34f, 0.12f, 402f),
            Quaternion.Euler(0f, 27f, 0f),
            new Vector3(0.95f, 1f, 0.95f));

        Place(
            "RiverRoadLakeFall/Lake01.prefab",
            lakes,
            "Upper_Right_Lake",
            new Vector3(266f, 0.12f, 402f),
            Quaternion.Euler(0f, -18f, 0f),
            new Vector3(0.95f, 1f, 0.95f));

        Vector3[] lakeRocks =
        {
            new Vector3(11f, 0.1f, 86f),
            new Vector3(50f, 0.1f, 105f),
            new Vector3(249f, 0.1f, 108f),
            new Vector3(289f, 0.1f, 87f),
            new Vector3(12f, 0.1f, 417f),
            new Vector3(56f, 0.1f, 390f),
            new Vector3(244f, 0.1f, 389f),
            new Vector3(288f, 0.1f, 418f)
        };

        for (int i = 0; i < lakeRocks.Length; i++)
        {
            Place(
                "Rock/Rock0" + ((i % 9) + 1) + ".prefab",
                rocks,
                "Lake_Rock_" + (i + 1).ToString("00"),
                lakeRocks[i],
                Quaternion.Euler(0f, i * 37f, 0f),
                Vector3.one * 1.15f);
        }

        Vector3[] flowerPatches =
        {
            new Vector3(58f, 0.05f, 82f),
            new Vector3(20f, 0.05f, 115f),
            new Vector3(241f, 0.05f, 82f),
            new Vector3(280f, 0.05f, 115f),
            new Vector3(62f, 0.05f, 420f),
            new Vector3(238f, 0.05f, 420f)
        };

        for (int i = 0; i < flowerPatches.Length; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                float angle = j * 60f * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * 3.5f,
                    0f,
                    Mathf.Sin(angle) * 3.5f);

                Place(
                    "TreePlants/Flower0" + ((j % 5) + 1) + ".prefab",
                    plants,
                    "Lake_Flower_" + i.ToString("00") + "_" + j.ToString("00"),
                    flowerPatches[i] + offset,
                    Quaternion.Euler(0f, j * 43f, 0f),
                    Vector3.one * 1.05f);
            }
        }
    }

    private static void BuildUpperTemple(
        Transform structures,
        Transform effects,
        Transform rocks)
    {
        Place(
            "BuildingUtilityDeco/Gate04.prefab",
            structures,
            "Upper_Temple_Main_Gate",
            new Vector3(150f, 0.14f, 368f),
            Quaternion.identity,
            new Vector3(1.65f, 1.35f, 1.65f));

        Place(
            "BuildingUtilityDeco/Portal03.prefab",
            structures,
            "Upper_Temple_Exit_Portal",
            new Vector3(150f, 0.2f, 427f),
            Quaternion.Euler(0f, 180f, 0f),
            Vector3.one * 1.45f);

        Vector3[] pillars =
        {
            new Vector3(118f, 0.12f, 382f),
            new Vector3(182f, 0.12f, 382f),
            new Vector3(118f, 0.12f, 418f),
            new Vector3(182f, 0.12f, 418f)
        };

        for (int i = 0; i < pillars.Length; i++)
        {
            Place(
                "BuildingUtilityDeco/Pillar0" + ((i % 4) + 1) + ".prefab",
                structures,
                "Temple_Pillar_" + (i + 1).ToString("00"),
                pillars[i],
                Quaternion.identity,
                Vector3.one * 1.35f);
        }

        for (int i = 0; i < 5; i++)
        {
            Place(
                "BuildingUtilityDeco/Wall0" + ((i % 4) + 1) + ".prefab",
                structures,
                "Temple_Left_Wall_" + (i + 1).ToString("00"),
                new Vector3(105f, 0.1f, 378f + i * 12f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1.3f, 1.15f, 1.3f));

            Place(
                "BuildingUtilityDeco/Wall0" + (((i + 1) % 4) + 1) + ".prefab",
                structures,
                "Temple_Right_Wall_" + (i + 1).ToString("00"),
                new Vector3(195f, 0.1f, 378f + i * 12f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1.3f, 1.15f, 1.3f));
        }

        Place(
            "BuildingUtilityDeco/Crystal01.prefab",
            effects,
            "Temple_Crystal_Left",
            new Vector3(132f, 0.18f, 420f),
            Quaternion.identity,
            Vector3.one * 1.15f);

        Place(
            "BuildingUtilityDeco/Crystal01.prefab",
            effects,
            "Temple_Crystal_Right",
            new Vector3(168f, 0.18f, 420f),
            Quaternion.identity,
            Vector3.one * 1.15f);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Temple_Fire_Left",
            new Vector3(126f, 0.12f, 377f),
            Quaternion.identity,
            Vector3.one);

        Place(
            "BuildingUtilityDeco/Fire01.prefab",
            effects,
            "Temple_Fire_Right",
            new Vector3(174f, 0.12f, 377f),
            Quaternion.identity,
            Vector3.one);

        Place(
            "Rock/CobbleStoneCircle01.prefab",
            rocks,
            "Temple_Cobble_Center",
            TempleCenter,
            Quaternion.identity,
            new Vector3(3.6f, 1f, 3.6f));
    }

    private static void BuildMountainBorder(Transform mountains, Transform rocks)
    {
        Vector3[] mountainPositions =
        {
            new Vector3(-45f, 0.05f, -28f),
            new Vector3(-46f, 0.05f, 47f),
            new Vector3(-46f, 0.05f, 125f),
            new Vector3(-47f, 0.05f, 272f),
            new Vector3(-45f, 0.05f, 388f),
            new Vector3(345f, 0.05f, -28f),
            new Vector3(346f, 0.05f, 47f),
            new Vector3(346f, 0.05f, 125f),
            new Vector3(347f, 0.05f, 272f),
            new Vector3(345f, 0.05f, 388f),
            new Vector3(2f, 0.05f, 446f),
            new Vector3(78f, 0.05f, 447f),
            new Vector3(222f, 0.05f, 447f),
            new Vector3(298f, 0.05f, 446f)
        };

        for (int i = 0; i < mountainPositions.Length; i++)
        {
            string mountainPrefab = i % 3 == 0
                ? "Mountains/RockMountain0" + ((i % 7) + 1) + ".prefab"
                : "Mountains/Mountain0" + ((i % 7) + 1) + ".prefab";

            Place(
                mountainPrefab,
                mountains,
                "Border_Mountain_" + (i + 1).ToString("00"),
                mountainPositions[i],
                Quaternion.Euler(0f, i * 47f, 0f),
                Vector3.one * Mathf.Lerp(1.05f, 1.45f, (i % 5) / 4f));
        }

        Vector3[] cliffPositions =
        {
            new Vector3(58f, 0.08f, 249f),
            new Vector3(142f, 0.08f, 249f),
            new Vector3(158f, 0.08f, 249f),
            new Vector3(242f, 0.08f, 249f),
            new Vector3(57f, 0.08f, 351f),
            new Vector3(143f, 0.08f, 351f),
            new Vector3(157f, 0.08f, 351f),
            new Vector3(243f, 0.08f, 351f)
        };

        for (int i = 0; i < cliffPositions.Length; i++)
        {
            Place(
                "Rock/RockCliff0" + ((i % 6) + 1) + ".prefab",
                rocks,
                "Bridge_Gap_Cliff_" + (i + 1).ToString("00"),
                cliffPositions[i],
                Quaternion.Euler(0f, i % 2 == 0 ? 0f : 180f, 0f),
                Vector3.one * 1.25f);
        }
    }

    private static void ScatterForestAndDetails(
        Transform trees,
        Transform rocks,
        Transform plants)
    {
        System.Random random = new System.Random(1600400);

        ScatterObjects(
            random,
            trees,
            105,
            "TreePlants/Tree0{0}.prefab",
            1,
            5,
            0.85f,
            1.35f,
            true);

        ScatterObjects(
            random,
            rocks,
            55,
            "Rock/Rock{0:00}.prefab",
            1,
            10,
            0.75f,
            1.35f,
            false);

        ScatterObjects(
            random,
            plants,
            150,
            "TreePlants/Grass0{0}.prefab",
            1,
            7,
            0.75f,
            1.25f,
            false);

        ScatterObjects(
            random,
            plants,
            55,
            "TreePlants/Flower0{0}.prefab",
            1,
            5,
            0.8f,
            1.2f,
            false);
    }

    private static void ScatterObjects(
        System.Random random,
        Transform parent,
        int count,
        string prefabPattern,
        int minVariant,
        int maxVariant,
        float minScale,
        float maxScale,
        bool treesOnly)
    {
        int placed = 0;
        int attempts = 0;

        while (placed < count && attempts < count * 80)
        {
            attempts++;

            bool upperRow = random.NextDouble() > 0.72;
            float x = Mathf.Lerp(-42f, 342f, (float)random.NextDouble());
            float z = upperRow
                ? Mathf.Lerp(356f, 444f, (float)random.NextDouble())
                : Mathf.Lerp(-42f, 244f, (float)random.NextDouble());

            Vector3 position = new Vector3(x, treesOnly ? 0.03f : 0.05f, z);

            if (IsReservedForGameplay(position))
                continue;

            int variant = random.Next(minVariant, maxVariant + 1);
            string relativePath = string.Format(prefabPattern, variant);
            float scale = Mathf.Lerp(minScale, maxScale, (float)random.NextDouble());

            Place(
                relativePath,
                parent,
                parent.name + "_Scatter_" + (placed + 1).ToString("000"),
                position,
                Quaternion.Euler(0f, random.Next(0, 360), 0f),
                Vector3.one * scale);

            placed++;
        }
    }

    private static bool IsReservedForGameplay(Vector3 position)
    {
        Vector2 point = new Vector2(position.x, position.z);

        if (DistanceToPolyline(point, MainPath) < 18f)
            return true;

        if (DistanceToPolyline(point, LeftBranchPath) < 16f)
            return true;

        if (DistanceToPolyline(point, RightBranchPath) < 16f)
            return true;

        if (Vector2.Distance(point, new Vector2(PlayerSpawn.x, PlayerSpawn.z)) < 28f)
            return true;

        if (Vector2.Distance(point, new Vector2(SmallArenaCenter.x, SmallArenaCenter.z)) < 34f)
            return true;

        if (Vector2.Distance(point, new Vector2(LeftArenaCenter.x, LeftArenaCenter.z)) < 47f)
            return true;

        if (Vector2.Distance(point, new Vector2(RightArenaCenter.x, RightArenaCenter.z)) < 47f)
            return true;

        if (Mathf.Abs(position.x - 100f) < 15f && position.z > 225f && position.z < 374f)
            return true;

        if (Mathf.Abs(position.x - 200f) < 15f && position.z > 225f && position.z < 374f)
            return true;

        if (Vector2.Distance(point, new Vector2(TempleCenter.x, TempleCenter.z)) < 48f)
            return true;

        if (Vector2.Distance(point, new Vector2(32f, 92f)) < 25f)
            return true;

        if (Vector2.Distance(point, new Vector2(268f, 93f)) < 25f)
            return true;

        if (Vector2.Distance(point, new Vector2(34f, 402f)) < 24f)
            return true;

        if (Vector2.Distance(point, new Vector2(266f, 402f)) < 24f)
            return true;

        return false;
    }

    private static float DistanceToPolyline(Vector2 point, List<Vector3> polyline)
    {
        float bestDistance = float.MaxValue;

        for (int i = 0; i < polyline.Count - 1; i++)
        {
            Vector2 a = new Vector2(polyline[i].x, polyline[i].z);
            Vector2 b = new Vector2(polyline[i + 1].x, polyline[i + 1].z);
            bestDistance = Mathf.Min(bestDistance, DistanceToSegment(point, a, b));
        }

        return bestDistance;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 segment = b - a;
        float lengthSquared = segment.sqrMagnitude;

        if (lengthSquared < 0.0001f)
            return Vector2.Distance(point, a);

        float t = Mathf.Clamp01(Vector2.Dot(point - a, segment) / lengthSquared);
        Vector2 projection = a + segment * t;
        return Vector2.Distance(point, projection);
    }

    private static void PlaceCobbleRing(
        Transform parent,
        Vector3 center,
        float radius,
        int count,
        string prefix)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 position = center + new Vector3(
                Mathf.Cos(radians) * radius,
                0.04f,
                Mathf.Sin(radians) * radius);

            Place(
                "Rock/Cobble0" + ((i % 6) + 1) + ".prefab",
                parent,
                prefix + "_" + (i + 1).ToString("00"),
                position,
                Quaternion.Euler(0f, -angle, 0f),
                Vector3.one * 1.1f);
        }
    }

    private static void BuildGameplayMarkers(Transform parent)
    {
        CreateMarker(parent, "PlayerSpawn", PlayerSpawn, 3);
        CreateMarker(parent, "Fork_Point", ForkCenter + Vector3.up * 0.4f, 4);

        GameObject smallGroup = NewChild(parent.gameObject, "Small_Encounter_4_Enemies");
        CreateMarker(smallGroup.transform, "SmallArena_Center", SmallArenaCenter, 1);
        CreateEnemyRing(smallGroup.transform, SmallArenaCenter, 13f, 4, 45f, "EnemySpawn");

        GameObject leftGroup = NewChild(parent.gameObject, "Left_Arena_15_Enemies");
        CreateMarker(leftGroup.transform, "LeftArena_Center", LeftArenaCenter, 1);
        CreateFifteenEnemyMarkers(leftGroup.transform, LeftArenaCenter);

        GameObject rightGroup = NewChild(parent.gameObject, "Right_Arena_15_Enemies");
        CreateMarker(rightGroup.transform, "RightArena_Center", RightArenaCenter, 1);
        CreateFifteenEnemyMarkers(rightGroup.transform, RightArenaCenter);

        CreateMarker(
            parent,
            "LevelExit_UpperTemple",
            new Vector3(150f, 0.45f, 425f),
            2);
    }

    private static void CreateFifteenEnemyMarkers(Transform parent, Vector3 center)
    {
        CreateEnemyRing(parent, center, 13f, 5, 18f, "InnerEnemy");
        CreateEnemyRing(parent, center, 25f, 6, 0f, "MiddleEnemy");
        CreateEnemyRing(parent, center, 35f, 4, 45f, "OuterEnemy");
    }

    private static void CreateEnemyRing(
        Transform parent,
        Vector3 center,
        float radius,
        int count,
        float startAngle,
        string prefix)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * (360f / count);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 position = center + new Vector3(
                Mathf.Cos(radians) * radius,
                0.45f,
                Mathf.Sin(radians) * radius);

            CreateMarker(
                parent,
                prefix + "_" + (i + 1).ToString("00"),
                position,
                0);
        }
    }

    private static void CreateMarker(
        Transform parent,
        string name,
        Vector3 position,
        int iconIndex)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, true);
        marker.transform.position = position;

        GUIContent iconContent = EditorGUIUtility.IconContent(
            "sv_label_" + Mathf.Clamp(iconIndex, 0, 7));

        if (iconContent != null && iconContent.image is Texture2D icon)
            EditorGUIUtility.SetIconForObject(marker, icon);
    }

    private static void BuildLightingAndAtmosphere(Transform parent)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.48f, 0.64f, 0.66f, 1f);
        RenderSettings.fogDensity = 0.0015f;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.58f, 0.71f, 0.76f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.38f, 0.48f, 0.41f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.17f, 0.22f, 0.18f, 1f);

        GameObject sun = new GameObject("Directional Light");
        sun.transform.SetParent(parent, true);
        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

        Light sunLight = sun.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.color = new Color(1f, 0.92f, 0.78f, 1f);
        sunLight.intensity = 1.18f;
        sunLight.shadows = LightShadows.Soft;
        sunLight.shadowStrength = 0.82f;

        GameObject fill = new GameObject("Soft Fill Light");
        fill.transform.SetParent(parent, true);
        fill.transform.rotation = Quaternion.Euler(55f, 140f, 0f);

        Light fillLight = fill.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.48f, 0.62f, 0.78f, 1f);
        fillLight.intensity = 0.25f;
        fillLight.shadows = LightShadows.None;

        GameObject previewCamera = new GameObject("EDITOR_PREVIEW_CAMERA_DISABLED");
        previewCamera.transform.SetParent(parent, true);
        previewCamera.transform.position = new Vector3(385f, 310f, -210f);
        previewCamera.transform.rotation = Quaternion.Euler(43f, -36f, 0f);

        Camera camera = previewCamera.AddComponent<Camera>();
        camera.enabled = false;
        camera.fieldOfView = 47f;
        camera.farClipPlane = 1400f;
        previewCamera.tag = "Untagged";
    }

    private static void PlaceRoadPolyline(
        Transform parent,
        string prefix,
        List<Vector3> points,
        string roadPrefab,
        float widthScale)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            PlaceRoadLine(
                parent,
                prefix + "_Part_" + (i + 1).ToString("00"),
                points[i],
                points[i + 1],
                roadPrefab,
                widthScale);
        }
    }

    private static void PlaceRoadLine(
        Transform parent,
        string prefix,
        Vector3 start,
        Vector3 end,
        string roadPrefab,
        float widthScale)
    {
        Vector3 direction = end - start;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance < 1f)
            return;

        Vector3 forward = direction / distance;
        int count = Mathf.Max(1, Mathf.CeilToInt(distance / 12.5f));
        float segmentLength = distance / count;
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        for (int i = 0; i < count; i++)
        {
            Vector3 position = start + forward * (segmentLength * (i + 0.5f));

            Place(
                roadPrefab,
                parent,
                prefix + "_" + (i + 1).ToString("00"),
                position,
                rotation,
                new Vector3(
                    widthScale,
                    1f,
                    segmentLength / RoadOriginalLength));
        }
    }

    private static GameObject Place(
        string relativePath,
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale)
    {
        string fullPath = BasePath + relativePath;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        GameObject instance;

        if (prefab != null)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }
        else
        {
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "MISSING_" + name;
            instance.transform.localScale = new Vector3(3f, 0.5f, 3f);
            Debug.LogWarning("Missing prefab: " + fullPath);
        }

        if (instance == null)
            throw new InvalidOperationException("Could not create object: " + name);

        instance.name = name;
        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        return instance;
    }

    private static GameObject NewRoot(string name)
    {
        return new GameObject(name);
    }

    private static GameObject NewChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    private static void EnsureOutputFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }
}
#endif
