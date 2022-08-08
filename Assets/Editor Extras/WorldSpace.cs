using UnityEngine;

public class WorldSpace : Core
{
	public GameObject container_Floor;
	public Transform contTranform_Floor { get { return container_Floor.transform; } }
	public GameObject container_Collectibles;
	public Transform contTranform_Collectibles { get { return container_Collectibles.transform; } }

	public void Reset()
    {
		if (container_Floor == null)
		{
			container_Floor = new GameObject("Floor");
			contTranform_Floor.SetParent(transform);
		}
		else
		{
			for (int i = contTranform_Floor.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(contTranform_Floor.GetChild(i).gameObject);
			}
		}
		if (container_Collectibles == null)
		{
			container_Collectibles = new GameObject("Collectibles");
			contTranform_Collectibles.SetParent(transform);
		}
		else
		{
			for (int i = contTranform_Collectibles.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(contTranform_Collectibles.GetChild(i).gameObject);
			}
		}
    }
}
