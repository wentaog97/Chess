// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ChessPiecePrefabManager : MonoBehaviour
// {
//     public MovesManager movesManager;
//     public GameManager gameManager;
//     public GameObject[] tiles;
//     private bool isDragging = false;
//     private Vector3 offset;
//     private Vector3 ori, tar;

//     // ChessManager cm;
    
//     void Update()
//     {
//         // if (isDragging)
//         // {
//         //     Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//         //     transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);
//         // }
//     }

//     void OnMouseDown()
//     {
//         // isDragging = true;
//         // Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//         // offset = transform.position - mousePosition;
//     }

//     void OnMouseUp()
//     {
//         // isDragging = false;
//         // SnapToNearestTile();
//     }

//     GameObject SnapToNearestTile()
//     {
//         GameObject closestTile = null;
//         float minDistance = float.MaxValue;
//         Vector3 closestTilePosition = Vector3.zero;

//         foreach (GameObject tile in tiles)
//         {
//             float distance = Vector3.Distance(transform.position, tile.transform.position);
//             if (distance < minDistance)
//             {
//                 closestTile = tile;
//                 minDistance = distance;
//                 closestTilePosition = tile.transform.position;
//             }
//         }

//         transform.position = new Vector3(closestTilePosition.x, closestTilePosition.y, transform.position.z);
        
//         return closestTile;
//     }
// }

