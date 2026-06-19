using UnityEngine;
using TMPro;

public class TileInfo : MonoBehaviour
{
    public int tileX;
    public int tileZ;

    [Header("Materials")]
    public Material defaultMaterial;
    public Material hoverMaterial;
    public Material selectedMaterial;

    private Renderer tileRenderer;
    private TextMeshProUGUI tileInfoText;
    private bool isTarget;

    public void SetTilePosition(int x, int z)
    {
        tileX = x;
        tileZ = z;
        gameObject.name = "Tile No: " + x + ", " + z;
    }

    void Start()
    {
        tileRenderer = GetComponentInChildren<Renderer>();

        GameObject ui = GameObject.Find("TileInfoUI");
        if (ui != null) tileInfoText = ui.GetComponent<TextMeshProUGUI>();
    }

    private void OnMouseEnter()
    {
        if (tileInfoText != null)
            tileInfoText.text = "Grid Position: (" + tileX + ", " + tileZ + ")";

        if (!isTarget && tileRenderer != null && hoverMaterial != null)
            tileRenderer.material = hoverMaterial;
    }

    private void OnMouseExit()
    {
        // keep the highlight if this is the selected tile
        if (!isTarget && tileRenderer != null && defaultMaterial != null)
            tileRenderer.material = defaultMaterial;
    }

    public void HighlightAsTarget()
    {
        isTarget = true;
        if (tileRenderer != null && selectedMaterial != null)
            tileRenderer.material = selectedMaterial;
    }

    public void ClearHighlight()
    {
        isTarget = false;
        if (tileRenderer != null && defaultMaterial != null)
            tileRenderer.material = defaultMaterial;
    }
}
