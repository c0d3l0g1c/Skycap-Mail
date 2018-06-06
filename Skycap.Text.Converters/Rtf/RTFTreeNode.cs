namespace Skycap.Text.Converters.Rtf
{
    using System;

    public class RTFTreeNode : RTFObject
    {
        private RTFTreeNode a;
        private RTFTreeNode b;
        private RTFNodeCollection c;

        public RTFTreeNode(RTFObject rtfObject)
        {
            this.c = new RTFNodeCollection();
            base.ObjectType = rtfObject.ObjectType;
            base.Key = rtfObject.Key;
            base.HasParam = rtfObject.HasParam;
            base.Param = rtfObject.Param;
        }

        public RTFTreeNode(RTFObjectType type) : this(type, "", false, 0)
        {
        }

        public RTFTreeNode(RTFObjectType type, string key, bool hasParam, int param)
        {
            this.c = new RTFNodeCollection();
            base.ObjectType = type;
            base.Key = key;
            base.HasParam = hasParam;
            base.Param = param;
        }

        public void Append(RTFTreeNode node)
        {
            node.a = this;
            node.b = this.b;
            this.c.Add(node);
        }

        public void Remove(int index)
        {
            this.c.RemoveAt(index);
        }

        public RTFNodeCollection Children
        {
            get
            {
                return this.c;
            }
        }

        public RTFTreeNode FirstChild
        {
            get
            {
                if (this.c.Count == 0)
                {
                    return null;
                }
                return this.c[0];
            }
        }

        public RTFTreeNode Parent
        {
            get
            {
                return this.a;
            }
        }

        public RTFTreeNode Root
        {
            get
            {
                return this.b;
            }
        }
    }
}

