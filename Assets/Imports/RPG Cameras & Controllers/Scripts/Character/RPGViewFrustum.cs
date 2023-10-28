using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JohnStairs.RCC.Character {
    public class RPGViewFrustum : MonoBehaviour {
        /// <summary>
        /// Controls how camera occlusion checks are performed
        /// </summary>
        public FrustumShape Shape = FrustumShape.Pyramid;
        /// <summary>
        /// Number of evenly distributed rays per near frustum plane edge, exclusive the plane corners (pyramid shape only)
        /// </summary>
        public int RaysPerEdge = 1;
        /// <summary>
        /// Only objects in these layers are processed by the view frustum. They either cause a camera zoom or are faded out if a tag of "TagsForFadeOut" is assigned to them
        /// </summary>
        public LayerMask OccludingLayers = 1;
        /// <summary>
        /// Controls if the logic of fading objects is based on tags or layers
        /// </summary>
        public ObjectTriggerOption FadeObjectsBy;
        /// <summary>
        /// If an object has one of these tags assigned, it is faded out when occluding the camera pivot
        /// </summary>
        public List<string> TagsForFading = new List<string>() { "FadeOut" };
        /// <summary>
        /// If an object is part of this layer, it is faded out when occluding the camera pivot
        /// </summary>
        public LayerMask LayersForFading = 0;
        /// <summary>
        /// Defines margins for the viewport in x and y direction
        /// </summary>
        public Vector2 ViewportMargin = new Vector2(0.2f, 0.2f);
        /// <summary>
        /// Enables/disables the camera look up functionality when touching the terrain
        /// </summary>
        public bool EnableCameraLookUp = true;
        /// <summary>
        /// Controls if the logic for the camera look up is based on tags or layers
        /// </summary>
        public ObjectTriggerOption LookUpTrigger;
        /// <summary>
        /// Scene objects with this tag assigned potentially trigger the camera look up functionality
        /// </summary>
        public List<string> TagsCausingLookUp = new List<string>() { "Terrain" };
        /// <summary>
        /// If an object is part of this layer, it can trigger a camera look up
        /// </summary>
        public LayerMask LayersCausingLookUp = 0;
        /// <summary>
        /// The alpha to which objects fade out when they enter the view frustum
        /// </summary>
        public float FadeOutAlpha = 0.2f;
        /// <summary>
        /// The alpha to which objects fade back in after they left the view frustum
        /// </summary>
        public float FadeInAlpha = 1.0f;
        /// <summary>
        /// The fade out duration of objects which have entered the view frustum
        /// </summary>
        public float FadeOutDuration = 0.2f;
        /// <summary>
        /// The fade in duration of objects which have left the view frustum
        /// </summary>
        public float FadeInDuration = 0.2f;
        /// <summary>
        /// If set to true, the character starts to fade when the camera's distance to its pivot is smaller than the "Character Fade Start Distance"
        /// </summary>
        public bool EnableCharacterFading = true;
        /// <summary>
        /// The alpha value of the character when the "CharacterFadeEndDistance" has been reached
        /// </summary>
        public float CharacterFadeOutAlpha = 0;
        /// <summary>
        /// The distance between the camera and its pivot where the character fading starts
        /// </summary>
        public float CharacterFadeStartDistance = 1.0f;
        /// <summary>
        /// The distance between the camera and its pivot where the character is faded out to "CharacterFadeOutAlpha"
        /// </summary>
        public float CharacterFadeEndDistance = 0.3f;

        /// <summary>
        /// The used camera (temporarily saved for gizmo purposes only)
        /// </summary>
        protected Camera _cameraToUse;
        /// <summary>
        /// Shift to the upper left corner etc. of the near clip plane
        /// </summary>
        protected Vector3 _shiftUpperLeft;
        /// <summary>
        /// Shift to the upper right corner etc. of the near clip plane
        /// </summary>
        protected Vector3 _shiftUpperRight;
        /// <summary>
        /// Shift to the lower left corner etc. of the near clip plane
        /// </summary>
        protected Vector3 _shiftLowerLeft;
        /// <summary>
        /// Shift to the lower right corner etc. of the near clip plane
        /// </summary>
        protected Vector3 _shiftLowerRight;
        /// <summary>
        /// Half near clip plane width
        /// </summary>
        protected float _halfWidth;
        /// <summary>
        /// Half near clip plane height
        /// </summary>
        protected float _halfHeight;
        /// <summary>
        /// Matrix for story the rays to be cast from the near frustum plane (pyramid shape only)
        /// </summary>
        protected Vector3[,] _rayMatrix;
        /// <summary>
        /// Contains the objects to fade from the last frame that are currently faded out
        /// </summary>
        protected SortedDictionary<int, GameObject> _previousObjectsToFade = new SortedDictionary<int, GameObject>();
        /// <summary>
        /// Contains all currently active fade out coroutines
        /// </summary>
        protected Dictionary<int, IEnumerator> _fadeOutCoroutines = new Dictionary<int, IEnumerator>();
        /// <summary>
        /// Contains all currently active fade in coroutines
        /// </summary>
        protected Dictionary<int, IEnumerator> _fadeInCoroutines = new Dictionary<int, IEnumerator>();
        /// <summary>
        /// Contains all renderes attached to the character which should be faded out
        /// </summary>
        protected Renderer[] _characterRenderersToFade;

        /// <summary>
        /// Enum for describing the shape of the view frustum
        /// </summary>
        public enum FrustumShape {
            Pyramid,
            Cuboid
        }

        /// <summary>
        /// Enum for holding functionality trigger options 
        /// </summary>
        public enum ObjectTriggerOption {
            Tag,
            Layer,
            Component
        }

        protected virtual void Start() {
            if (FadeObjectsBy == ObjectTriggerOption.Layer) {
                for (int i = 0; i < 32; i++) {
                    if (Utils.LayerInLayerMask(i, LayersForFading)) {
                        if (!Utils.LayerInLayerMask(i, OccludingLayers)) {
                            // Layer for fading is not part of the occluding layers => throw a warning
                            Debug.LogWarning("Layer \"" + LayerMask.LayerToName(i) + "\" is set up for fading but not part of the occluding layers! Consider adding it when you want it to fade");
                        }
                    }
                }
            }

            if (LookUpTrigger == ObjectTriggerOption.Layer) {
                for (int i = 0; i < 32; i++) {
                    if (Utils.LayerInLayerMask(i, LayersCausingLookUp)) {
                        if (!Utils.LayerInLayerMask(i, OccludingLayers)) {
                            // Layer for camera look up is not part of the occluding layers => throw a warning
                            Debug.LogWarning("Layer \"" + LayerMask.LayerToName(i) + "\" is set up for causing the camera look up but is not part of the occluding layers! Consider adding it");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the view frustum by setting the internal variables for the frustum planes and edges
        /// </summary>
        /// <param name="from">Beginning of the view frustum</param>
        /// <param name="to">End of the view frustum</param>
        /// <param name="cameraToUse">Camera to use for computations</param>
        protected virtual void Initialize(Vector3 from, Vector3 to, Camera cameraToUse) {
            _cameraToUse = cameraToUse;

            float halfFieldOfView = cameraToUse.fieldOfView * 0.5f * Mathf.Deg2Rad;
            _halfHeight = cameraToUse.nearClipPlane * Mathf.Tan(halfFieldOfView);
            _halfWidth = _halfHeight * cameraToUse.aspect + ViewportMargin.x;
            // Now add the viewport margin
            _halfHeight += ViewportMargin.y;

            Vector3 targetDirection = from - to;
            targetDirection.Normalize();

            Vector3 localRight = cameraToUse.transform.right;
            Vector3 localUp = Vector3.Cross(-targetDirection, localRight);
            localUp.Normalize();

            Vector3 offset = targetDirection * cameraToUse.nearClipPlane;

            _shiftUpperLeft = -localRight * _halfWidth + localUp * _halfHeight + offset;
            _shiftUpperRight = localRight * _halfWidth + localUp * _halfHeight + offset;
            _shiftLowerLeft = -localRight * _halfWidth - localUp * _halfHeight + offset;
            _shiftLowerRight = localRight * _halfWidth - localUp * _halfHeight + offset;
        }

        /// <summary>
        /// Checks for objects inside the view frustum and - depending on the handling - fades them out or lets the camera zoom in. 
        /// Returns -1 if there is no ambient occlusion, otherwise returns the closest possible distance so that the sight to the target is clear
        /// </summary>
        /// <param name="from">Beginning of the view frustum</param>
        /// <param name="to">End of the view frustum</param>
        /// <param name="cameraToUse">Camera to use for computations</param>
        /// <returns>The closest possible distance so that the sight to the target is clear</returns>
        public virtual float CheckForOcclusion(Vector3 from, Vector3 to, Camera cameraToUse) {
            float closestDistance = Mathf.Infinity;

            if (from == to) {
                // No occlusion for the empty distance
                return closestDistance;
            }

            // Compute the view frustum direction and length
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (distance <= cameraToUse.nearClipPlane) {
                // Do not check the distance which is anyhow not rendered
                return closestDistance;
            }

            direction.Normalize();

            // Set up the view frustum
            Initialize(from, to, cameraToUse);

            if (Shape == FrustumShape.Pyramid) {
                // PYRAMID VIEW FRUSTUM
                // Build ray matrix
                int maxIndex = RaysPerEdge + 1;
                _rayMatrix = new Vector3[maxIndex + 1, maxIndex + 1];

                // Corners
                _rayMatrix[0, 0] = to + _shiftUpperLeft;
                _rayMatrix[0, maxIndex] = to + _shiftUpperRight;
                _rayMatrix[maxIndex, 0] = to + _shiftLowerLeft;
                _rayMatrix[maxIndex, maxIndex] = to + _shiftLowerRight;

                Vector3 down = -(_shiftUpperLeft - _shiftLowerLeft);
                Vector3 right = (_shiftUpperRight - _shiftUpperLeft);

                for (int i = 1; i < maxIndex; i++) {
                    _rayMatrix[i, 0] = _rayMatrix[0, 0] + down * (i / (float)maxIndex);
                    _rayMatrix[i, maxIndex] = _rayMatrix[i, 0] + right;
                }

                for (int i = 0; i <= maxIndex; i++) {
                    for (int j = 1; j < maxIndex; j++) {
                        _rayMatrix[i, j] = _rayMatrix[i, 0] + right * (j / (float)maxIndex);
                    }
                }

                RaycastHit[] hitArray;
                RaycastHit hit;
                Vector3 rayDirection;
                // Loop over all rays in the matrix and cast them
                for (int i = 0; i <= maxIndex; i++) {
                    for (int j = 0; j <= maxIndex; j++) {
                        rayDirection = _rayMatrix[i, j] - from;
                        hitArray = Physics.RaycastAll(from, rayDirection, rayDirection.magnitude, OccludingLayers, QueryTriggerInteraction.Ignore);

                        if (hitArray.Length > 0) {
                            // Objects got hit, sort the hits by their distance to start
                            Array.Sort(hitArray, RaycastHitComparator);

                            for (int n = 0; n < hitArray.Length; n++) {
                                hit = hitArray[n];

                                if (FadeObject(hit.transform.gameObject)) {
                                    // Skip objects which should be faded out
                                    continue;
                                }

                                // Project the distance from the frustum edge onto the camera direction
                                float projectedDistance = Vector3.Project(rayDirection.normalized * hit.distance, direction).magnitude;

                                if (projectedDistance < closestDistance) {
                                    closestDistance = projectedDistance;
                                    // Draw debug line to the hit point
                                    Debug.DrawLine(from, hit.point, Color.red);
                                }
                                // We had a possible hit but it was not closer than a previously found closest distance
                                // Since hit arrays are sorted  => break and check the next ray
                                break;
                            }
                        }
                    }
                }
            } else {
                // CUBOID VIEW FRUSTUM
                // Cast the box which acts as the cuboid view frustum
                RaycastHit[] hitArray = BoxCastAll(from, to + direction * cameraToUse.nearClipPlane);

                // Objects got hit, sort the hits by their distance to start
                Array.Sort(hitArray, RaycastHitComparator);

                RaycastHit hit;
                for (int n = 0; n < hitArray.Length; n++) {
                    hit = hitArray[n];

                    if (hit.point == Vector3.zero) {
                        // Most likely due to the note described on https://docs.unity3d.com/ScriptReference/Physics.BoxCastAll.html
                        //Debug.LogWarning("There is a collider overlapping the box at the start of the sweep!");
                        // Skip this case
                        continue;
                    }

                    if (!FadeObject(hit.transform.gameObject)) {
                        // Hit object should not be faded out => causes a zoom in
                        closestDistance = hit.distance;
                        // Draw debug line to the hit point
                        Debug.DrawLine(from, hit.point, Color.red);
                        break;
                    }
                }
            }

            return closestDistance;
        }

        /// <summary>
        /// Handles the fading of qualified objects between from and to
        /// </summary>
        /// <param name="from">Beginning of the area to check</param>
        /// <param name="to">End of the area to check</param>
        /// <param name="cameraToUse">Camera to use for computations</param>
        public virtual void HandleObjectFading(Vector3 from, Vector3 to, Camera cameraToUse) {
            if (EnableCharacterFading) {
                // Let the character fade in/out
                CharacterFade(cameraToUse);
            }

            if (from == to) {
                // No occlusion for the empty distance
                return;
            }

            Initialize(from, to, cameraToUse);

            SortedDictionary<int, GameObject> objectsToFade = new SortedDictionary<int, GameObject>();

            #region Check for objects            
            RaycastHit[] hitArray = BoxCastAll(from, to);

            // Objects got hit, sort the hits by their distance to start
            Array.Sort(hitArray, RaycastHitComparator);

            RaycastHit hit;
            for (int n = 0; n < hitArray.Length; n++) {
                hit = hitArray[n];

                if (hit.point == Vector3.zero) {
                    // Most likely due to the note described on https://docs.unity3d.com/ScriptReference/Physics.BoxCastAll.html                        
                    //Debug.LogWarning("There is a collider overlapping the box at the start of the sweep!");
                    // Skip this case
                    continue;
                }

                if (!FadeObject(hit.transform.gameObject)) {
                    // Object should not be faded out => skip
                    continue;
                }

                int hitObjectID = hit.transform.GetInstanceID();

                if (!objectsToFade.ContainsKey(hitObjectID)) {
                    // Hit object is tagged for fading out and not yet tracked => fade it 
                    objectsToFade.Add(hitObjectID, hit.transform.gameObject);
                }
            }
            #endregion Check for objects

            #region Handle object fading
            // Create lists for objects to fade in or out
            List<GameObject> fadeOut = new List<GameObject>();
            List<GameObject> fadeIn = new List<GameObject>();

            // The following lines do the following: 
            // - Compare the objects to fade of the last frame and the objects hit in this frame
            // - If an object is in _previousObjectsToFade but not in objectsToFade, fade it back in (as it is no longer inside the view frustum)
            // - If an object is not in _previousObjectsToFade but in objectsToFade, fade it out (as it entered the view frustum this frame)
            // - If an object is in both lists, do nothing and continue (as the object was already inside the view frustum and still is)
            SortedDictionary<int, GameObject>.Enumerator i = _previousObjectsToFade.GetEnumerator();
            SortedDictionary<int, GameObject>.Enumerator j = objectsToFade.GetEnumerator();

            bool iFinished = !i.MoveNext();
            bool jFinished = !j.MoveNext();
            bool aListFinished = iFinished || jFinished;

            while (!aListFinished) {
                int iKey = i.Current.Key;
                int jKey = j.Current.Key;

                if (iKey == jKey) {
                    iFinished = !i.MoveNext();
                    jFinished = !j.MoveNext();
                    aListFinished = iFinished || jFinished;
                } else if (iKey < jKey) {
                    if (i.Current.Value != null) {
                        fadeIn.Add(i.Current.Value);
                    }
                    aListFinished = !i.MoveNext();
                    iFinished = true;
                    jFinished = false;
                } else {
                    if (j.Current.Value != null) {
                        fadeOut.Add(j.Current.Value);
                    }
                    aListFinished = !j.MoveNext();
                    iFinished = false;
                    jFinished = true;
                }
            }

            if (iFinished && !jFinished) {
                do {
                    if (j.Current.Value != null) {
                        fadeOut.Add(j.Current.Value);
                    }
                } while (j.MoveNext());
            } else if (!iFinished && jFinished) {
                do {
                    if (i.Current.Value != null) {
                        fadeIn.Add(i.Current.Value);
                    }
                } while (i.MoveNext());
            }

            foreach (GameObject o in fadeOut) {
                int objectID = o.transform.GetInstanceID();
                // Create a new coroutine for fading out the object
                IEnumerator coroutine = FadeObjectCoroutine(FadeOutAlpha, FadeOutDuration, o);

                // Check if there is a running fade in coroutine for this object
                if (_fadeInCoroutines.TryGetValue(objectID, out IEnumerator runningCoroutine)) {
                    // Stop the already running coroutine
                    StopCoroutine(runningCoroutine);
                    // Remove it from the fade in coroutines
                    _fadeInCoroutines.Remove(objectID);
                }
                // Add the new fade out coroutine to the list of fade out coroutines
                _fadeOutCoroutines.Add(objectID, coroutine);
                // Start the coroutine
                StartCoroutine(coroutine);
            }

            foreach (GameObject o in fadeIn) {
                int objectID = o.transform.GetInstanceID();
                // Create a new coroutine for fading in the object
                IEnumerator coroutine = FadeObjectCoroutine(FadeInAlpha, FadeInDuration, o);

                // Check if there is a running fade out coroutine for this object
                if (_fadeOutCoroutines.TryGetValue(objectID, out IEnumerator runningCoroutine)) {
                    // Stop the already running coroutine
                    StopCoroutine(runningCoroutine);
                    // Remove it from the fade out coroutines
                    _fadeOutCoroutines.Remove(objectID);
                }
                // Add the new fade in coroutine to the list of fade in coroutines
                _fadeInCoroutines.Add(objectID, coroutine);
                // Start the coroutine
                StartCoroutine(coroutine);
            }

            // Set the _previousObjectsToFade for the next frame occlusion computations
            _previousObjectsToFade = objectsToFade;
            #endregion Handle object fading
        }

        /// <summary>
        /// Comparator for comparing two RaycastHits by their distance
        /// </summary>
        /// <param name="a">Left-side RaycastHit</param>
        /// <param name="b">Right-side RaycastHit</param>
        /// <returns>A signed number indicating the relative values of a and b</returns>
		protected virtual int RaycastHitComparator(RaycastHit a, RaycastHit b) {
            return a.distance.CompareTo(b.distance);
        }

        /// <summary>
        /// Checks if a game object can be faded according to the set up tags/layers/component
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if the object should be faded, otherwise false</returns>
        protected virtual bool FadeObject(GameObject obj) {
            if (FadeObjectsBy == ObjectTriggerOption.Tag) {
                return TagsForFading.Contains(obj.transform.tag);
            } else if (FadeObjectsBy == ObjectTriggerOption.Layer) {
                return Utils.LayerInLayerMask(obj.layer, LayersForFading);
            } else {
                return obj.GetComponent<FadeOut>();
            }
        }

        /// <summary>
        /// Checks if a game object causes a camera look up according to the set up tags/layers/component
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if the object causes a look up, otherwise false</returns>
        protected virtual bool IsObjectCausingLookUp(GameObject obj) {
            if (LookUpTrigger == ObjectTriggerOption.Tag) {
                return TagsCausingLookUp.Contains(obj.transform.tag);
            } else if (FadeObjectsBy == ObjectTriggerOption.Layer) {
                return Utils.LayerInLayerMask(obj.layer, LayersCausingLookUp);
            } else {
                return obj.GetComponent<CauseCameraLookUp>();
            }
        }

        /// <summary>
        /// Casts a box such that all collisions between from and to are detected 
        /// </summary>
        /// <param name="from">Beginning of the detecting box</param>
        /// <param name="to">End of the detecting box</param>
        /// <returns>All ray cast hits between from and to</returns>
        private RaycastHit[] BoxCastAll(Vector3 from, Vector3 to) {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            direction.Normalize();
            // Set the box dimensions
            Vector3 boxHalfExtents = new Vector3(_halfWidth, _halfHeight, _cameraToUse.nearClipPlane * 0.5f);
            // Start the box center before the view frustum beginning to bypass colliders that overlap the box at the start of the sweep
            Vector3 boxCenter = from - direction * boxHalfExtents.z;
            Quaternion boxOrientation = Quaternion.LookRotation(direction);
            float maxDistance = distance - _cameraToUse.nearClipPlane;
            // Draw debug ray for the box cast, i.e. from the first box center to the last box center (end box position)
            //Debug.DrawRay(boxCenter, direction * maxDistance, Color.magenta);
            // Cast the box
            return Physics.BoxCastAll(boxCenter, boxHalfExtents, direction, boxOrientation, maxDistance, OccludingLayers, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Creates a coroutine for fading an object to a given alpha value over a given duration
        /// </summary>
        /// <param name="target">Target alpha value</param>
        /// <param name="duration">Duration for fading</param>
        /// <param name="o">Game object to fade</param>
        /// <returns>Coroutine object</returns>
		protected virtual IEnumerator FadeObjectCoroutine(float target, float duration, GameObject o) {
            bool continueFading = true;
            int objectID = o.transform.GetInstanceID();
            // Get all renderers of object o
            Renderer[] objectRenderers = o.transform.GetComponentsInChildren<Renderer>();

            if (objectRenderers.Length > 0) {
                if (target == FadeOutAlpha) {
                    foreach (Renderer r in objectRenderers) {
                        Utils.DisableZWrite(r);
                    }
                }

                // There are renderers to fade, create a current velocity array for each renderer fade
                float[] currentVelocity = new float[objectRenderers.Length];

                while (continueFading) {
                    for (int i = 0; i < objectRenderers.Length; i++) {
                        if (!o) {
                            // Object was destroyed in the meantime
                            continueFading = false;
                            break;
                        }

                        Renderer r = objectRenderers[i];
                        if (!r) {
                            continue;
                        }

                        Material[] mats = r.materials;
                        float alpha = -1.0f;

                        foreach (Material m in mats) {
                            // Check for standard (built-in) render pipeline or Universal Render Pipeline color properties
                            if (m.HasProperty("_Color") || m.HasProperty("_BaseColor")) {
                                if (alpha == -1.0f) {
                                    // Compute the alpha only once
                                    alpha = Mathf.SmoothDamp(m.color.a, target, ref currentVelocity[i], duration);
                                }

                                // Apply the modified alpha value
                                Color color = m.color;
                                color.a = alpha;
                                m.color = color;
                            }
                        }

                        r.materials = mats;

                        if (Utils.IsAlmostEqual(alpha, target, 0.01f)) {
                            // The current alpha is almost equal to the target alpha value to => stop fading
                            continueFading = false;
                        }
                    }

                    // Continue computation in the next frame
                    yield return null;
                } // end of while loop

                if (target == FadeInAlpha) {
                    foreach (Renderer r in objectRenderers) {
                        Utils.EnableZWrite(r);
                    }
                }
            }
            // Coroutine done or object was destroyed => remove the coroutine from both coroutine lists
            _fadeOutCoroutines.Remove(objectID);
            _fadeInCoroutines.Remove(objectID);
        }

        /// <summary>
        /// Lets the character fade depending on the position of usedCamera
        /// </summary>
        /// <param name="cameraToUse">Camera for calculating the alpha to which the character should fade</param>
		protected virtual void CharacterFade(Camera cameraToUse) {
            UpdateCharacterRenderersToFade();

            Collider collider = GetComponent<Collider>();
            Vector3 closestPointToCharacter = transform.position;
            if (collider) {
                closestPointToCharacter = collider.ClosestPointOnBounds(cameraToUse.transform.position);
            }

            // Get the actual distance between the used camera and the character
            float actualDistance = Vector3.Distance(closestPointToCharacter, cameraToUse.transform.position);

            // Compute the new alpha value depending on the fading start and end distance
            float t = Mathf.Clamp01((actualDistance - CharacterFadeEndDistance) / (CharacterFadeStartDistance - CharacterFadeEndDistance));

            // Go through all renderers found for the character
            foreach (Renderer r in _characterRenderersToFade) {
                // Go through all their materials
                foreach (Material m in r.materials) {
                    // Adjust their color's alpha value accordingly
                    Color color = m.color;
                    // Interpolate the new alpha t between the minimun and maximum possible alpha
                    color.a = Mathf.SmoothStep(CharacterFadeOutAlpha, 1.0f, t);
                    // Set for standard (built-in) render pipeline
                    m.SetColor("_Color", color);
                    // Set for URP (Universal Render Pipeline)
                    m.SetColor("_BaseColor", color);
                }
            }
        }

        /// <summary>
        /// Updates the _characterRenderersToFade, has to be called when the character renderers changed and character fading is on
        /// </summary>
		protected virtual void UpdateCharacterRenderersToFade() {
            List<Renderer> renderers = new List<Renderer>();
            Renderer[] temp = GetComponentsInChildren<Renderer>();

            bool addRenderer = true;
            foreach (Renderer r in temp) {
                foreach (Material m in r.materials) {
                    if (m.HasProperty("_Color")) {
                        // Add only if the color property is available
                        if (addRenderer) {
                            addRenderer = false;
                            renderers.Add(r);
                        }
                        m.SetInt("_ZWrite", 1);
                    }
                }
                addRenderer = true;
            }

            _characterRenderersToFade = renderers.ToArray();
        }

        /// <summary>
        /// Checks if the given camera is touching the ground
        /// </summary>
        /// <param name="cameraToUse">Camera to check</param>
        /// <returns>True if it is touching the ground</returns>
		public virtual bool IsTouchingGround(Camera cameraToUse, Vector3 target) {
            if (!EnableCameraLookUp) {
                return false;
            }

            return Physics.Raycast(cameraToUse.transform.position, Vector3.down, out RaycastHit hitInfo, 1.0f + ViewportMargin.y, OccludingLayers, QueryTriggerInteraction.Ignore)
                    && IsObjectCausingLookUp(hitInfo.transform.gameObject)
                    && cameraToUse.transform.position.y < target.y;
        }

        /// <summary>
        /// Draws the view frustum via debugging lines
        /// </summary>
        /// <param name="from">Beginning of the frustum</param>
        /// <param name="to">End of the frustum</param>
        /// <param name="cameraToUse">Camera used for the computating frustum planes and edges</param>
        /// <param name="withCameraPlane">If true, the camera plane at the cameraToUse's position is drawn additionally</param>
        public virtual void DrawFrustum(Vector3 from, Vector3 to, Camera cameraToUse, bool withCameraPlane = false) {
            if (from == to) {
                return;
            }

            Initialize(from, to, cameraToUse);

            Color frustumPlaneColor = Color.gray;
            Color cameraPlaneColor = Color.yellow;
            Color frustumEdgeColor = Color.white;

            // Calculate the near frustum plane at the end position (e.g. desired camera position)
            Vector3 upperLeft = to + _shiftUpperLeft;
            Vector3 upperRight = to + _shiftUpperRight;
            Vector3 lowerLeft = to + _shiftLowerLeft;
            Vector3 lowerRight = to + _shiftLowerRight;
            // Draw the frustum plane at the end
            Debug.DrawLine(upperLeft, upperRight, frustumPlaneColor);
            Debug.DrawLine(upperLeft, lowerLeft, frustumPlaneColor);
            Debug.DrawLine(upperRight, lowerRight, frustumPlaneColor);
            Debug.DrawLine(lowerLeft, lowerRight, frustumPlaneColor);

            if (Shape == FrustumShape.Pyramid) {
                Debug.DrawLine(upperLeft, from, frustumEdgeColor);
                Debug.DrawLine(upperRight, from, frustumEdgeColor);
                Debug.DrawLine(lowerLeft, from, frustumEdgeColor);
                Debug.DrawLine(lowerRight, from, frustumEdgeColor);
            } else {
                // Calculate the near frustum plane at the start position (e.g. pivot position)
                upperLeft = from + _shiftUpperLeft;
                upperRight = from + _shiftUpperRight;
                lowerLeft = from + _shiftLowerLeft;
                lowerRight = from + _shiftLowerRight;
                // Draw the frustum plane at the start
                Debug.DrawLine(upperLeft, upperRight, frustumPlaneColor);
                Debug.DrawLine(upperLeft, lowerLeft, frustumPlaneColor);
                Debug.DrawLine(upperRight, lowerRight, frustumPlaneColor);
                Debug.DrawLine(lowerLeft, lowerRight, frustumPlaneColor);

                Vector3 frustumDirection = to - from;
                Debug.DrawRay(upperLeft, frustumDirection, frustumEdgeColor);
                Debug.DrawRay(upperRight, frustumDirection, frustumEdgeColor);
                Debug.DrawRay(lowerLeft, frustumDirection, frustumEdgeColor);
                Debug.DrawRay(lowerRight, frustumDirection, frustumEdgeColor);
            }

            if (withCameraPlane) {
                // Calculate the near frustum plane at the current camera position
                Vector3 cameraPosition = cameraToUse.transform.position;
                upperLeft = cameraPosition + _shiftUpperLeft;
                upperRight = cameraPosition + _shiftUpperRight;
                lowerLeft = cameraPosition + _shiftLowerLeft;
                lowerRight = cameraPosition + _shiftLowerRight;
                // Draw the frustum plane at the camera position
                Debug.DrawLine(upperLeft, upperRight, cameraPlaneColor);
                Debug.DrawLine(upperLeft, lowerLeft, cameraPlaneColor);
                Debug.DrawLine(upperRight, lowerRight, cameraPlaneColor);
                Debug.DrawLine(lowerLeft, lowerRight, cameraPlaneColor);
            }
        }
    }
}
