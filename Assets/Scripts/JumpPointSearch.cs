using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPointSearch
{
    public static Vector2 BORDER_MIN = new Vector2(-3.5f, -11.5f);
    public static Vector2 BORDER_MAX = new Vector2(18.5f, 9.5f);
    public static float GRID_SIZE = 1.0f;
    public static float COLLIDER_RADIOUS = 0.5f; //should be half of the grid size

    private static Dictionary<double, Vector2> pPair;
    
    private static bool IsInsideBox(Vector2 point, Vector2 boxMin, Vector2 boxMax)
    {
        float x = point.x;
        float y = point.y;
        return (x >= boxMin.x) && (x <= boxMax.x) &&
                (y >= boxMin.y) && (y <= boxMax.y);
    }

    private static bool IsValid(float x, float y)
    {
        Vector2 point = new Vector2(x, y);
        return IsInsideBox(point, BORDER_MIN, BORDER_MAX);
    }

    private static bool IsUnBlocked(float x, float y)
    {
        Vector2 point = new Vector2(x, y);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(point, COLLIDER_RADIOUS);

        if (hitColliders.Length == 0)
        {
            return true;
        }
        else
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider2D hitCollider = hitColliders[i];
                GameObject gameObject = hitCollider.gameObject;
                string tag = gameObject.tag;
                
                if (!tag.Equals("AICustomer") && !tag.Equals("Security"))
                {
                    return false;
                }
            }
            return true;
        }
    }

    private static bool IsValidAndUnblocked(float x, float y)
    {
        return IsValid(x, y) && IsUnBlocked(x, y);
    }

    private static bool IsDestination(float x, float y, Vector2 dest)
    {
        if (x.Equals(dest.x) && y.Equals(dest.y))
            return true;
        else
            return false;
    }

    private static double CalculateHValue(float x, float y, Vector2 dest)
    {
        return Math.Pow(x - dest.x, 2) + Math.Pow(y - dest.y, 2);
    }

    private static List<Vector2> SmoothPath(List<Vector2> path)
    {
        if (path.Count < 2)
        {
            return path;
        }

        for (int j = path.Count - 1; j > 1; j--)
        {
            int startIndex = j;
            Vector2 start = path[startIndex];

            for (int i = 0; i < path.Count - 2; i++)
            {
                Vector2 end = path[i];
                Vector2 direction = new Vector2(end.x - start.x, end.y - start.y).normalized;
                float distance = (start - end).magnitude;

                RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, LayerMask.GetMask("Default"));
                if (hit.collider == null)
                {
                    int removeCount = startIndex - i - 1;
                    path.RemoveRange(i + 1, removeCount);
                    j = j - removeCount;
                    break;
                }
            }
        }

        return path;
    }

    private static List<Vector2> TracePath(Dictionary<string, CellDetail> cellDetails, Vector2 dest)
    {
        float x = dest.x;
        float y = dest.y;
        string key = GetListKey(x, y);

        List<Vector2> path = new List<Vector2>();

        while (!(cellDetails[key].parentI.Equals(x)
                && cellDetails[key].parentJ.Equals(y)))
        {
            path.Add(new Vector2(x, y));
            x = cellDetails[key].parentI;
            y = cellDetails[key].parentJ;
            key = GetListKey(x, y);
        }

        path.Add(new Vector2(x, y));

        return SmoothPath(path);
    }

    private static string GetListKey(float x, float y)
    {
        return x + "_" + y;
    }

    private static List<Vector2> ProcessSuccessor(float i,
        float j,
        float nextI,
        float nextJ,
        string currentKey,
        Vector2 dest,
        float cellDistance,
        List<VectorP> openList,
        Dictionary<string, Vector2> closedList,
        Dictionary<string, CellDetail> cellDetails
        )
    {
        string nextKey = GetListKey(nextI, nextJ);
        if (IsValid(nextI, nextJ) == true)
        {
            // If the destination cell is the same as the
            // current successor
            if (IsDestination(nextI, nextJ, dest) == true)
            {
                // Set the Parent of the destination cell
                if (!cellDetails.ContainsKey(nextKey))
                {
                    cellDetails.Add(nextKey, new CellDetail(i, j));
                }
                else
                {
                    cellDetails[nextKey].parentI = i;
                    cellDetails[nextKey].parentJ = j;
                }
                return TracePath(cellDetails, dest);
            }
            // If the successor is already on the closed
            // list or if it is blocked, then ignore it.
            // Else do the following
            else if (closedList.ContainsKey(nextKey) == false &&
                    IsUnBlocked(nextI, nextJ) == true)
            {
                double gNew = cellDetails[currentKey].g + cellDistance;
                double hNew = CalculateHValue(nextI, nextJ, dest);
                double fNew = gNew + hNew;

                // If it isn�t on the open list, add it to
                // the open list. Make the current square
                // the parent of this square. Record the
                // f, g, and h costs of the square cell
                //                OR
                // If it is on the open list already, check
                // to see if this path to that square is better,
                // using 'f' cost as the measure.
                if (!cellDetails.ContainsKey(nextKey))
                {
                    openList.Add(new VectorP(fNew,
                            new Vector2(nextI, nextJ)));

                    cellDetails.Add(nextKey, new CellDetail(i, j, fNew, gNew, hNew));
                }
                else if (cellDetails[nextKey].f > fNew)
                {
                    openList.Add(new VectorP(fNew,
                            new Vector2(nextI, nextJ)));

                    // Update the details of this cell
                    cellDetails[nextKey].f = fNew;
                    cellDetails[nextKey].g = gNew;
                    cellDetails[nextKey].h = hNew;
                    cellDetails[nextKey].parentI = i;
                    cellDetails[nextKey].parentJ = j;
                }
            }
        }
        return null;
    }

    private static Vector2 Direction(float parentI, float parentJ, float currentI, float currentJ)
    {
        Vector2 directionVector = new Vector2(Math.Sign(currentI - parentI), Math.Sign(currentJ - parentJ));
        return directionVector;
    }

    private static List<Vector2> Neighbors(float parentI, float parentJ, float currentI, float currentJ)
    {
        List<Vector2> neighborCells = new List<Vector2>();
        // add all neighbors if the current node is the source node
        if (parentI.Equals(currentI) && parentJ.Equals(currentJ))
        {
            if (IsValidAndUnblocked(currentI, currentJ - GRID_SIZE))
                neighborCells.Add(new Vector2(currentI, currentJ - GRID_SIZE));
            if (IsValidAndUnblocked(currentI, currentJ + GRID_SIZE))
                neighborCells.Add(new Vector2(currentI, currentJ + GRID_SIZE));
            if (IsValidAndUnblocked(currentI + GRID_SIZE, currentJ))
                neighborCells.Add(new Vector2(currentI + GRID_SIZE, currentJ));
            if (IsValidAndUnblocked(currentI - GRID_SIZE, currentJ))
                neighborCells.Add(new Vector2(currentI - GRID_SIZE, currentJ));
            if (IsValidAndUnblocked(currentI + GRID_SIZE, currentJ - GRID_SIZE))
                neighborCells.Add(new Vector2(currentI + GRID_SIZE, currentJ - GRID_SIZE));
            if (IsValidAndUnblocked(currentI - GRID_SIZE, currentJ - GRID_SIZE))
                neighborCells.Add(new Vector2(currentI - GRID_SIZE, currentJ - GRID_SIZE));
            if (IsValidAndUnblocked(currentI + GRID_SIZE, currentJ + GRID_SIZE))
                neighborCells.Add(new Vector2(currentI + GRID_SIZE, currentJ + GRID_SIZE));
            if (IsValidAndUnblocked(currentI - GRID_SIZE, currentJ + GRID_SIZE))
                neighborCells.Add(new Vector2(currentI - GRID_SIZE, currentJ + GRID_SIZE));

            return neighborCells;
        }

        Vector2 direction = Direction(parentI, parentJ, currentI, currentJ);
        float dx = direction.x;
        float dy = direction.y;
        // diagonal prune
        if (!dx.Equals(0f) && !dy.Equals(0f))
        {
            if (IsValidAndUnblocked(currentI, currentJ + dy * GRID_SIZE))
            {
                neighborCells.Add(new Vector2(currentI, currentJ + dy * GRID_SIZE));
            }
            if (IsValidAndUnblocked(currentI + dx * GRID_SIZE, currentJ))
            {
                neighborCells.Add(new Vector2(currentI + dx * GRID_SIZE, currentJ));
            }
            if (IsValidAndUnblocked(currentI + dx * GRID_SIZE, currentJ + dy * GRID_SIZE))
            {
                neighborCells.Add(new Vector2(currentI + dx * GRID_SIZE, currentJ + dy * GRID_SIZE));
            }
            if (!IsValidAndUnblocked(currentI - dx * GRID_SIZE, currentJ) && IsValidAndUnblocked(currentI - dx * GRID_SIZE, currentJ + dy * GRID_SIZE))
            {
                neighborCells.Add(new Vector2(currentI - dx * GRID_SIZE, currentJ + dy * GRID_SIZE));
            }
            if (!IsValidAndUnblocked(currentI, currentJ - dy * GRID_SIZE) && IsValidAndUnblocked(currentI + dx * GRID_SIZE, currentJ - dy * GRID_SIZE))
            {
                neighborCells.Add(new Vector2(currentI + dx * GRID_SIZE, currentJ - dy * GRID_SIZE));
            }
        }
        // straight line prune
        else
        {
            // vertical prune
            if (dx.Equals(0f))
            {
                if (IsValidAndUnblocked(currentI, currentJ + dy * GRID_SIZE))
                {
                    neighborCells.Add(new Vector2(currentI, currentJ + dy * GRID_SIZE));
                }
                if (!IsValidAndUnblocked(currentI + GRID_SIZE, currentJ) &&
                    IsValidAndUnblocked(currentI + GRID_SIZE, currentJ + dy * GRID_SIZE))
                {
                    neighborCells.Add(new Vector2(currentI + GRID_SIZE, currentJ + dy * GRID_SIZE));
                }
                if (!IsValidAndUnblocked(currentI - GRID_SIZE, currentJ) &&
                    IsValidAndUnblocked(currentI - GRID_SIZE, currentJ + dy * GRID_SIZE))
                {
                    neighborCells.Add(new Vector2(currentI - GRID_SIZE, currentJ + dy * GRID_SIZE));
                }
            }
            // horizontal prune
            else
            {
                if (IsValidAndUnblocked(currentI + dx * GRID_SIZE, currentJ))
                {
                    neighborCells.Add(new Vector2(currentI + dx * GRID_SIZE, currentJ));
                }
                if (!IsValidAndUnblocked(currentI, currentJ + GRID_SIZE))
                {
                    neighborCells.Add(new Vector2(currentI + dx * GRID_SIZE, currentJ + GRID_SIZE));
                }
                if (!IsValidAndUnblocked(currentI, currentJ - GRID_SIZE))
                {
                    neighborCells.Add(new Vector2(currentI + dx * GRID_SIZE, currentJ - GRID_SIZE));
                }
            }
        }

        return neighborCells;
    }

    private static Vector2 Jump(Vector2 initialNode, Vector2 direction, Vector3 src, Vector3 dest)
    {
        Vector2 nextNode = initialNode + direction;
        if (!IsValidAndUnblocked(nextNode.x, nextNode.y))
        {
            return new Vector2(float.NaN, float.NaN);
        }

        if (IsDestination(nextNode.x, nextNode.y, dest))
        {
            return nextNode;
        }

        // check if the node has forced neighbors
        float dx = direction.x;
        float dy = direction.y;
        float currentI = nextNode.x;
        float currentJ = nextNode.y;
        // diagonal scan
        if (!dx.Equals(0f) && !dy.Equals(0f))
        {
            if (!IsUnBlocked(currentI - dx * GRID_SIZE, currentJ) && IsUnBlocked(currentI - dx * GRID_SIZE, currentJ + dy * GRID_SIZE))
            {
                return nextNode;
            }
            if (!IsUnBlocked(currentI, currentJ - dy * GRID_SIZE) && IsUnBlocked(currentI + dx * GRID_SIZE, currentJ - dy * GRID_SIZE))
            {
                return nextNode;
            }
        }
        // straight line scan
        else
        {
            // vertical scan
            if (dx.Equals(0f))
            {
                if (!IsUnBlocked(currentI + GRID_SIZE, currentJ) &&
                    IsUnBlocked(currentI + GRID_SIZE, currentJ + dy * GRID_SIZE))
                {
                    return nextNode;
                }
                if (!IsUnBlocked(currentI - GRID_SIZE, currentJ) &&
                    IsUnBlocked(currentI - GRID_SIZE, currentJ + dy * GRID_SIZE))
                {
                    return nextNode;
                }
            }
            // horizontal scan
            else
            {
                if (!IsUnBlocked(currentI, currentJ + GRID_SIZE) &&
                    IsUnBlocked(currentI + dx * GRID_SIZE, currentJ + GRID_SIZE))
                {
                    return nextNode;
                }
                if (!IsUnBlocked(currentI, currentJ - GRID_SIZE) &&
                    IsUnBlocked(currentI + dx * GRID_SIZE, currentJ - GRID_SIZE))
                {
                    return nextNode;
                }
            }
        }

        if (!dx.Equals(0f) && !dy.Equals(0f))
        {
            Vector2 newDirection, newNode, point;
            for (int i = 0; i <= 2; i++)
            {
                float step = i * GRID_SIZE;
                // vertical scan
                newDirection = new Vector2(0, direction.y);
                newNode = new Vector2(nextNode.x, nextNode.y + Math.Sign(direction.y) * step);
                point = Jump(newNode, newDirection, src, dest);
                if (!float.IsNaN(point.x) && !float.IsNaN(point.y))
                {
                    return nextNode;
                }
                // horizontal scan
                newDirection = new Vector2(direction.x, 0);
                newNode = new Vector2(nextNode.x + Math.Sign(direction.x) * step, newNode.y);
                point = Jump(newNode, newDirection, src, dest);
                if (!float.IsNaN(point.x) && !float.IsNaN(point.y))
                {
                    return nextNode;
                }
                // diagonal scan
                newDirection = new Vector2(direction.x, direction.y);
                newNode = new Vector2(nextNode.x + Math.Sign(direction.x) * step, nextNode.y + Math.Sign(direction.y) * step);
                point = Jump(nextNode, newDirection, src, dest);
                if (!float.IsNaN(point.x) && !float.IsNaN(point.y))
                {
                    return nextNode;
                }
            }
        }

        return Jump(nextNode, direction, src, dest);
    }

    private static List<Vector2> IdentifySuccessors(float parentI, float parentJ, float currentI, float currentJ, Vector3 src, Vector3 dest)
    {
        List<Vector2> successors = new List<Vector2>();
        Vector2 currentNode = new Vector2(currentI, currentJ);
        List<Vector2> neighbors = Neighbors(parentI, parentJ, currentI, currentJ);
        // return all neighbors if the current node is the source node
        if (parentI.Equals(currentI) && parentJ.Equals(currentJ))
        {
            return neighbors;
        }

        foreach (var neighbor in neighbors)
        {
            Vector2 direction = Direction(currentI, currentJ, neighbor.x, neighbor.y);
            Vector2 node = Jump(currentNode, direction, src, dest);
            if (!float.IsNaN(node.x) && !float.IsNaN(node.y))
            {
                if (IsDestination(node.x, node.y, dest))
                {
                    List<Vector2> destNode = new List<Vector2>();
                    destNode.Add(node);
                    return destNode;
                }
                successors.Add(node);
            }
        }

        return successors;
    }

    public static List<Vector2> SearchPath(Vector3 src, Vector3 dest)
    {
        if (!IsValid(src.x, src.y) || !IsValid(dest.x, dest.y))
        {
            return null;
        }

        if (IsDestination(src.x, src.y, dest))
        {
            return null;
        }

        // Create a closed list and initialise it to false which means
        // that no cell has been included yet
        // This closed list is implemented as a boolean 2D array
        Dictionary<string, Vector2> closedList = new Dictionary<string, Vector2>();

        // Declare a 2D array of structure to hold the details
        //of that cell
        Dictionary<string, CellDetail> cellDetails = new Dictionary<string, CellDetail>();

        float i, j;
        float nextI, nextJ;
        string currentKey;
        List<Vector2> path;

        // Initialising the parameters of the starting node
        i = src.x;
        j = src.y;
        CellDetail srcCell = new CellDetail(i, j, 0.0, 0.0, 0.0);
        cellDetails.Add(GetListKey(i, j), srcCell);

        /*
        Create an open list having information as-
        <f, <i, j>>
        where f = g + h,
        and i, j are the row and column index of that cell
        Note that 0 <= i <= ROW-1 & 0 <= j <= COL-1
        This open list is implenented as a set of pair of pair.*/
        List<VectorP> openList = new List<VectorP>();

        // Put the starting cell on the open list and set its
        // 'f' as 0
        openList.Add(new VectorP(0.0, new Vector2(i, j)));

        while (openList.Count != 0)
        {
            VectorP p = openList[0];

            // Remove this vertex from the open list
            openList.RemoveAt(0);

            // Add this vertex to the closed list
            i = p.position.x;
            j = p.position.y;
            currentKey = GetListKey(i, j);
            if (!closedList.ContainsKey(currentKey))
            {
                closedList.Add(currentKey, p.position);
            }
            CellDetail cellDetail = cellDetails[currentKey];
            List<Vector2> successors = IdentifySuccessors(cellDetail.parentI, cellDetail.parentJ, i, j, src, dest);
            foreach (var successor in successors)
            {
                nextI = successor.x;
                nextJ = successor.y;
                path = ProcessSuccessor(i,
                    j,
                    nextI,
                    nextJ,
                    currentKey,
                    dest,
                    GRID_SIZE,
                    openList,
                    closedList,
                    cellDetails);
                if (path != null)
                {
                   // path.Reverse();
                    return path;
                }
            }
        }
        // When the destination cell is not found and the open
        // list is empty, then we conclude that we failed to
        // reach the destiantion cell. This may happen when the
        // there is no way to destination cell (due to blockages)
        return null;
    }

    private class VectorP
    {
        public double p;
        public Vector2 position;

        public VectorP(double p, Vector2 position)
        {
            this.p = p;
            this.position = position;
        }
    }

    private class CellDetail
    {
        public float parentI;
        public float parentJ;
        public double f;
        public double h;
        public double g;

        public CellDetail(float i, float j)
        {
            this.parentI = i;
            this.parentJ = j;
            this.f = Double.MaxValue;
            this.g = Double.MaxValue;
            this.h = Double.MaxValue;
        }

        public CellDetail(float i, float j, double f, double g, double h)
        {
            this.parentI = i;
            this.parentJ = j;
            this.f = f;
            this.g = g;
            this.h = h;
        }
    }
}