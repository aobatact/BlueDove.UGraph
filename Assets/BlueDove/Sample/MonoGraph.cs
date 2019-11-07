using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BlueDove.UGraph;
using BlueDove.UGraph.Algorithm;
using Unity.Mathematics;
using UnityEngine;
using BiEdge = BlueDove.UGraph.DirectionalEdge<BlueDove.Sample.MonoNode, BlueDove.Sample.MonoEdge>;

namespace BlueDove.Sample
{
    public class MonoGraph : MonoBehaviour, IGraph<MonoNode, BiEdge>, ICostFunc<BiEdge>
    {
        private IDPublisherS _idPublisher;
        private BidirectionalGraph<MonoNode, MonoEdge> _graph;

        [SerializeField]
        private MonoNode nodePrefab;

        [SerializeField]
        private MonoEdge edgePrefab;

        [SerializeField]
        private Color edgeSourceDefault;

        [SerializeField]
        private Color edgeTargetDefault;

        
        [SerializeField]
        private bool autoCreateNodes;

        [SerializeField]
        private Vector2 cellSize;

        [SerializeField]
        private Vector2Int cellCount;

        
        [SerializeField]
        private bool autoCreateEdges;

        [SerializeField]
        private float minDistSq;

        [SerializeField]
        private float minAngle;

        public MonoNode CreateNewNode(Vector3 pos)
        {
            var node = Instantiate(nodePrefab, pos, Quaternion.identity);
            node.ID = _idPublisher.Publish();
            return node;
        }

        public MonoEdge CreateEdge(MonoNode source, MonoNode target)
        {
            var edge = Instantiate(edgePrefab);
            edge.Source = source;
            edge.Target = target;
            _graph.AddEdge(edge);
            return edge;
        }

        // Start is called before the first frame update
        void Start()
        {
            _idPublisher = default;
            _graph = new BidirectionalGraph<MonoNode, MonoEdge>(4);
            var nodes = autoCreateNodes
                ? GraphUtils.GeneratePoints(cellSize, Unsafe.As<Vector2Int, int2>(ref cellCount), x =>
                {
                    var node = Instantiate(nodePrefab, Unsafe.As<float3, Vector3>(ref x), Quaternion.identity,
                        transform);
                    node.ID = _idPublisher.Publish();
                    return node;
                })
                : InitChildNodes();
            if (autoCreateEdges)
            {
                GraphUtils.CreateEdges(this, nodes, minDistSq,
                    minAngle, (x, y) =>
                    {
                        var e = Instantiate(edgePrefab, transform);
                        e.Source = x;
                        e.Target = y;
                        e.name = $"Edge [{e.Source.name}, {e.Target.name}]";
                        return new BiEdge(e, true);
                    });
            }
            else
            {
                InitChildEdges();
            }
        }

        MonoNode[] InitChildNodes()
        {
            var nodes = GetComponentsInChildren<MonoNode>();
            foreach (var node in nodes)
            {
                node.ID = _idPublisher.Publish();
            }

            return nodes;
        }

        void InitChildEdges()
        {
            var edges = GetComponentsInChildren<MonoEdge>();
            foreach (var edge in edges)
            {
                _graph.AddEdge(edge);
            }
        }


        public MonoNode GetNodeByID(int id)
            => _graph.GetNodes().FirstOrDefault(x => x.ID == id);

        public bool AcceptDuplicateEdges => false;


        public bool AddNode(MonoNode node) { return _graph.AddNode(node); }

        public bool AddEdge(BiEdge edge) => _graph.AddEdge(edge);

        public bool RemoveNode(MonoNode node) => _graph.RemoveNode(node);

        public bool RemoveEdge(BiEdge edge) => _graph.RemoveEdge(edge);

        public void Clear() => _graph.Clear();

        private void OnDestroy() => Clear();

        public float Calc(BiEdge value)
            => Vector3.Distance(value.Source.transform.position, value.Target.transform.position);

        public void ResetNodeColors()
        {
            foreach (var node in GetNodes())
            {
                node.Mark(Color.white);
            }
        }

        public void ResetEdgeColors()
        {
            foreach (var edge in GetEdges())
            {
                edge.Edge.Renderer.startColor = edgeSourceDefault;
                edge.Edge.Renderer.endColor = edgeTargetDefault;
            }
        }

        public bool Contains(MonoNode node) => _graph.Contains(node);

        public bool Contains(BiEdge edge) => _graph.Contains(edge);

        public BiEdge GetEdge(MonoNode source, MonoNode target, out bool found) =>
            _graph.GetEdge(source, target, out found);

        public IEnumerable<BiEdge> GetEdges() => _graph.GetEdges();

        public IEnumerable<BiEdge> GetEdges(MonoNode node) => _graph.GetEdges(node);

        public IEnumerable<MonoNode> GetNodes() => _graph.GetNodes();
    }
}
