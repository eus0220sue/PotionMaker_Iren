using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObsMap : MonoBehaviour
{
    /// <summary>
    /// ≈∏¿œ∏ 
    /// </summary>
    [SerializeField] private Tilemap m_tileMap  = null;

    /// <summary>
    /// ≈∏¿œ∏  π›»Ø
    /// </summary>
    public Tilemap IsGet { get { return m_tileMap; } }
}
