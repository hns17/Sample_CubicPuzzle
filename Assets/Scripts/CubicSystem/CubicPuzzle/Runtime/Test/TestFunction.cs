
using CubicSystem.CubicPuzzle;
using System.Collections.Generic;
using UnityEngine;


public class TestFunction
{
    public static void PrintDropTable(Dictionary<BlockModel, BlockPathData> pathTable)
    {
#if UNITY_EDITOR
        foreach(var item in pathTable) {
            string text = item.Key.Position + " => ";

            foreach(Vector2 pos in item.Value.PathData) {
                text += pos + " => ";
            }
            text += "End";

            Debug.Log(text);
        }
#endif
    }

    public static void PrintBlocksState(List<BlockModel> blocks)
    {
#if UNITY_EDITOR
        string text = string.Format("Count : {0}\n", blocks.Count);
        foreach(var block in blocks) {
            text += string.Format("Idx : {0}, Color : {1}, Position : {2}", block.Idx, block.Color, block.Position) + "\n";
        }

        Debug.Log(text);
#endif
    }

    public static void ModifyBoardBlock(StageInputManager inputManager, HexBoardModel Board)
    {
#if UNITY_EDITOR
        inputManager.InputActions.Editor.PressDown.started += (x =>
        {
            //Test Case 1
            //List<int> testIdx = new List<int> { 8, 9, 13, 14, 18, 19 };

            //Test Case 2
            //List<int> testIdx = new List<int>
            //{   2,7,12,17,22,
            //    3,8,13,18,23,
            //    4,9,14,19,24
            //};

            //Test Case 3
            List<int> testIdx = new List<int>();
            for(int i = 0; i < 25; i++) {
                testIdx.Add(i);
            }

            foreach(var idx in testIdx) {
                Board.Blocks[idx].SetMatchColor(MatchColorType.BLUE);
            }

        });
#endif
    }
}