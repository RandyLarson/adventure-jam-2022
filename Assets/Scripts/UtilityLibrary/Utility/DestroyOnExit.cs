using UnityEngine;

public class DestroyOnExit : MonoBehaviour
{
	private void OnTriggerExit2D(Collider2D collision)
	{
		var spawnOnDestroy = collision.gameObject.GetComponent<SpawnOnCreateOrDestroy>();
		if ( spawnOnDestroy != null)
        {
			spawnOnDestroy.DoSpawnItem = false;
        }

		Destroy(collision.gameObject);
	}
}
