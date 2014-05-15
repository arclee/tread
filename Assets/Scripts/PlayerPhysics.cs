using UnityEngine;
using System.Collections;


[RequireComponent (typeof(BoxCollider2D))]
public class PlayerPhysics : MonoBehaviour {
	
	public LayerMask collisionMask;

	private BoxCollider2D Bxcollider2D;
	private Vector3 s;
	private Vector3 c;
	
	private Vector3 originalSize;
	private Vector3 originalCentre;
	private float colliderScale;
	
	private int collisionDivisionsX = 3;
	private int collisionDivisionsY =10;
	
	private float skin = .005f;
	
	[HideInInspector]
	public bool grounded;
	[HideInInspector]
	public bool movementStopped;
	[HideInInspector]
	public bool canWallHold;

	private Transform platform;
	private Vector3 platformPositionOld;
	private Vector3 deltaPlatformPos;
	
	
	Ray ray;
	//RaycastHit hit;
	
	void Start() {
		Bxcollider2D = GetComponent<BoxCollider2D>();
		colliderScale = transform.localScale.x;
		
		originalSize = Bxcollider2D.size;
		originalCentre = Bxcollider2D.center;
		SetCollider(originalSize,originalCentre);
	}

	public void Move(Vector2 moveAmount, float moveDirX) {
		
		float deltaY = moveAmount.y;
		float deltaX = moveAmount.x;
		Vector2 p = transform.position;

		if (platform) {
			deltaPlatformPos = platform.position - platformPositionOld;
		}
		else {
			deltaPlatformPos = Vector3.zero;
		}

		#region Vertical Collisions
		// Check collisions above and below
		grounded = false;
		
		for (int i = 0; i<collisionDivisionsX; i ++)
		{
			float dir = Mathf.Sign(deltaY);
			float x = (p.x + c.x - s.x/2) + s.x/(collisionDivisionsX-1) * i; // Left, centre and then rightmost point of collider
			float y = p.y + c.y + s.y/2 * dir; // Bottom of collider

			Vector2 startp = new Vector2(x,y);
			Vector2 endp = new Vector2(x,y + dir);
			ray = new Ray(new Vector2(x,y), new Vector2(0,dir));
			//Debug.DrawRay(ray.origin,ray.direction);
			Debug.DrawLine(startp, endp);
			//if (Physics.Raycast(ray,out hit,Mathf.Abs(deltaY) + skin,collisionMask))
			RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(x, y), new Vector2(0, dir), Mathf.Abs(deltaY) + skin, collisionMask);
			if (rh2d.collider != null)
			{

				platform = rh2d.transform;
				platformPositionOld = platform.position;

				// Get Distance between player and ground
				float dst = Vector3.Distance (ray.origin, rh2d.point);
				
				// Stop player's downwards movement after coming within skin width of a collider
				if (dst > skin) {
					deltaY = dst * dir - skin * dir;
				}
				else {
					deltaY = 0;
				}
				
				grounded = true;
				
				break;
				
			}
			else {
				platform = null;
			}
		}
		#endregion
		#region Sideways Collisions
		// Check collisions left and right
		movementStopped = false;
		canWallHold = false;
		
		if (deltaX != 0)
		{
			for (int i = 0; i<collisionDivisionsY; i ++)
			{
				float dir = Mathf.Sign(deltaX);
				float x = p.x + c.x + s.x/2 * dir;
				float y = p.y + c.y - s.y/2 + s.y/(collisionDivisionsY-1) * i;
				
				ray = new Ray(new Vector2(x,y), new Vector2(dir,0));
				//Debug.DrawRay(ray.origin,ray.direction);

				RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(x, y), new Vector2(0, dir), Mathf.Abs(deltaY) + skin, collisionMask);
				//if (Physics.Raycast(ray,out hit,Mathf.Abs(deltaX) + skin,collisionMask))
				if (rh2d.collider != null)
				{
					
					if (rh2d.collider.tag == "Wall Jump") {
						
						if (Mathf.Sign(deltaX) == Mathf.Sign(moveDirX) && moveDirX != 0) {
							canWallHold = true;
						}
					}
					
					// Get Distance between player and ground
					float dst = Vector3.Distance (ray.origin, rh2d.point);
					
					// Stop player's downwards movement after coming within skin width of a collider
					if (dst > skin) {
						deltaX = dst * dir - skin * dir;
					}
					else {
						deltaX = 0;
					}
					
					movementStopped = true;
					break;
					
				}
			}
		}
		#endregion

		if (!grounded && !movementStopped) {
			Vector3 playerDir = new Vector3(deltaX,deltaY);
			Vector3 o = new Vector3(p.x + c.x + s.x/2 * Mathf.Sign(deltaX),p.y + c.y + s.y/2 * Mathf.Sign(deltaY));
			ray = new Ray(o,playerDir.normalized);
			
			if (Physics.Raycast(ray,Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY),collisionMask)) {
				grounded = true;
				deltaY = 0;
			}
		}


		Vector2 finalTransform = new Vector2(deltaX + deltaPlatformPos.x,deltaY);
		
		transform.Translate(finalTransform,Space.World);
	}
	
	// Set collider
	public void SetCollider(Vector3 size, Vector3 centre) {
		Bxcollider2D.size = size;
		Bxcollider2D.center = centre;
		
		s = size * colliderScale;
		c = centre * colliderScale;
	}
	
	public void ResetCollider() {
		SetCollider(originalSize,originalCentre);	
	}
	
}
