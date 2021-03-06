﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSetter : MonoBehaviour
{
    public Tile[,] AttachTileObjects(Tile[,] map, int tileScale, int tileOffset, Transform parent, int Xend, int Yend)
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if((x==0 & y==0) || (x==Xend && y == Yend))
                {
                    AttachPastureTile(map[x, y], tileScale, tileOffset, parent);
                    continue;
                }               

                AttachTile(map[x, y], tileScale, tileOffset, parent);

            }
        }

        //map = ConnectTileExits(map);


        return map;
    }

    //Tile[,] ConnectTileExits(Tile[,] map)
    //{
    //    for (int x = 0; x < map.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < map.GetLength(1); y++)
    //        {
    //            map[x, y].ConnectExits();
    //        }
    //    }

    //    return map;
    //}

    public void PlaceEndLevelObject(GameObject levelEndObject,int x, int y, int tileScale, int tileOffset)
    {
        Vector3 position = new Vector3((x + tileOffset) * tileScale, 0, (y + tileOffset) * tileScale);
        Instantiate(levelEndObject, position, transform.rotation);
    }

    void AttachPastureTile(Tile tile, int tileScale, int tileOffset,Transform parent)
    {
        GameObject t = null;
        TileObject to = null;
        bool hasAdjacentExit;
        int numOfExits = DetermineTileExits(tile, out hasAdjacentExit);

        if (numOfExits == 1) //single
        {
            t = Instantiate(TileManager.SINGLE[0].gameObject, parent);          
            to = t.GetComponent<TileObject>();
        }
        if (numOfExits == 2 && !hasAdjacentExit) //straight
        {
            t = Instantiate(TileManager.STRAIGHT[0].gameObject, parent);           
            to = t.GetComponent<TileObject>();
        }
        if (numOfExits == 2 && hasAdjacentExit) //cornor
        {
            t = Instantiate(TileManager.CORNER[0].gameObject, parent);
            to = t.GetComponent<TileObject>();
        }
        if (numOfExits == 3) //tjunction
        {
            t = Instantiate(TileManager.JUNCTION[0].gameObject, parent);
            to = t.GetComponent<TileObject>();
        }
        if (numOfExits == 4) //allway
        {
            t = Instantiate(TileManager.ALL[0].gameObject, parent);
            to = t.GetComponent<TileObject>();
        }

        PositionTile(t, tile.position, tileScale, tileOffset);
        to = OrientTile(to.code, tile.tileCode, to);
        to.thisTile = tile;
        tile.tileObject = to;
    }

    void AttachTile(Tile tile, int tileScale, int tileOffset, Transform parent)
    {
        GameObject t = null;
        TileObject to = null;
        bool hasAdjacentExit;
        int numOfExits = DetermineTileExits(tile, out hasAdjacentExit);
      
        //Debug.Log(tile.tileCode[0].ToString() + tile.tileCode[1].ToString() + tile.tileCode[2].ToString() + tile.tileCode[3].ToString() + " " + numOFExits.ToString() + hasAdjacentExit);
        // range between 1 and length so that pasture is never chosen as it can only be used for start and end
        if(numOfExits == 1) //single
        {
            t = Instantiate(TileManager.SINGLE[Random.Range(1, TileManager.SINGLE.Count)].gameObject, parent);
            to = t.GetComponent<TileObject>();    
        }
        if (numOfExits == 2 && !hasAdjacentExit) //straight
        {
            t = Instantiate(TileManager.STRAIGHT[Random.Range(1, TileManager.STRAIGHT.Count)].gameObject, parent);
            to = t.GetComponent<TileObject>(); 
        }
        if (numOfExits == 2 && hasAdjacentExit) //cornor
        {
            t = Instantiate(TileManager.CORNER[Random.Range(1, TileManager.CORNER.Count)].gameObject, parent); 
            to = t.GetComponent<TileObject>();  
        }
        if (numOfExits == 3) //tjunction
        {
            t = Instantiate(TileManager.JUNCTION[Random.Range(1, TileManager.JUNCTION.Count)].gameObject, parent);
            to = t.GetComponent<TileObject>();
        }
        if(numOfExits == 4) //allway
        {
            t = Instantiate(TileManager.ALL[Random.Range(1, TileManager.ALL.Count)].gameObject, parent);
            to = t.GetComponent<TileObject>(); 
        }        

        PositionTile(t, tile.position, tileScale, tileOffset);       
        to = OrientTile(to.code, tile.tileCode, to);
        to.thisTile = tile;
        tile.tileObject = to;

    }

    void PositionTile(GameObject g, Coord pos, int tileScale, int tileOffset)
    {
        g.transform.position = new Vector3((pos.tileX + tileOffset)* tileScale, 0, (pos.tileY +tileOffset) * tileScale);
    }

    TileObject OrientTile(int[] defaultCode, int[] toCode, TileObject t) 
    {
        
        //to code will always be a rotation of the default code
        for (int i = 0; i < 4; i++)
        {
            bool correctOrientation = true;
            for (int j = 0; j < defaultCode.Length; j++)
            {
                if(defaultCode[j] != toCode[j])
                {
                    // wow this is a bit shit hope it isnt longer than 4
                    correctOrientation = false;
                    int tmp = defaultCode[0];
                    defaultCode[0] = defaultCode[3];
                    defaultCode[3] = defaultCode[2];
                    defaultCode[2] = defaultCode[1];
                    defaultCode[1] = tmp;
                    t = ReassignExitsAndRotate(t);
                    break;
                }                
            }

            if (correctOrientation)
            {
                break;
            }
           

           
        }

        return t;
    }

    TileObject ReassignExitsAndRotate(TileObject t)
    {
        //same shitness
        Vector3 euler = new Vector3(0, -90, 0);
        t.asset.transform.Rotate(euler);
        t.transform.GetChild(2).Rotate(euler);
        Exit temp = t.exits[0];
        t.exits[0] = t.exits[3];
        t.exits[3] = t.exits[2];
        t.exits[2] = t.exits[1];
        t.exits[1] = temp;
        return t;

    }

    int DetermineTileExits(Tile tile, out bool hasAdjacentExit)
    {
        int numOFExits = 0;
        hasAdjacentExit = false;
        for (int i = 0; i < tile.tileCode.Length; i++)
        {
            if (tile.tileCode[i] == 1)
            {
                numOFExits++;
            }

            if (!hasAdjacentExit && i != 0 && ((tile.tileCode[i] == 1 && tile.tileCode[i - 1] == 1) || (tile.tileCode[0] == 1 && tile.tileCode[3] == 1)))
            {
                hasAdjacentExit = true;
            }
        }

        return numOFExits;
    }
}
