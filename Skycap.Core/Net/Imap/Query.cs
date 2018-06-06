namespace Skycap.Net.Imap
{
    using Skycap.Net.Imap.Sequences;
    using System;
    using System.Globalization;

    public class Query
    {
        protected string query;

        protected Query(string query)
        {
            this.query = query;
        }

        public static Query All()
        {
            return new Query("ALL");
        }

        public static Query Answered()
        {
            return new Query("ANSWERED");
        }

        public static Query BCC(string bcc)
        {
            return new Query(string.Format("BCC \"{0}\"", bcc));
        }

        public static Query Before(DateTime date)
        {
            return new Query(string.Format("BEFORE {0}", date.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture)));
        }

        public static Query Body(string body)
        {
            return new Query(string.Format("BODY \"{0}\"", body));
        }

        public static Query CC(string cc)
        {
            return new Query(string.Format("CC \"{0}\"", cc));
        }

        public static Query Deleted()
        {
            return new Query("DELETED");
        }

        public static Query Draft()
        {
            return new Query("DRAFT");
        }

        public static Query Flagged()
        {
            return new Query("FLAGGED");
        }

        public static Query From(string from)
        {
            return new Query(string.Format("FROM \"{0}\"", from));
        }

        public static Query Header(string fieldName, string value)
        {
            return new Query(string.Format("HEADER \"{0}\" \"{1}\"", fieldName, value));
        }

        public static Query Keyword(Skycap.Net.Imap.Keyword keywords)
        {
            return new Query(string.Format("KEYWORD {0}", keywords));
        }

        public static Query Larger(uint size)
        {
            return new Query(string.Format("LARGER {0}", size));
        }

        public static Query New()
        {
            return new Query("NEW");
        }

        public static Query Not(Query query)
        {
            return new Query(string.Format("NOT ({0})", query.query));
        }

        public static Query Old()
        {
            return new Query("OLD");
        }

        public static Query On(DateTime date)
        {
            return new Query(string.Format("ON {0}", date.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture)));
        }

        public static Query Or(Query left, Query right)
        {
            return new Query(string.Format("OR ({0}) ({1})", left.query, right.query));
        }

        public static Query Recent()
        {
            return new Query("RECENT");
        }

        public static Query Seen()
        {
            return new Query("SEEN");
        }

        public static Query SentBefore(DateTime date)
        {
            return new Query(string.Format("SENTBEFORE {0}", date.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture)));
        }

        public static Query SentOn(DateTime date)
        {
            return new Query(string.Format("SENTON {0}", date.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture)));
        }

        public static Query SentSince(DateTime date)
        {
            return new Query(string.Format("SENTSINCE {0}", date.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture)));
        }

        public static Query SequenceSet(ISequence sequence)
        {
            return new Query(sequence.ToString());
        }

        public static Query Since(DateTime date)
        {
            return new Query(string.Format("SINCE {0}", date.ToString("dd-MMM-yyyy", CultureInfo.CurrentCulture)));
        }

        public static Query Smaller(uint size)
        {
            return new Query(string.Format("SMALLER {0}", size));
        }

        public static Query Subject(string subject)
        {
            return new Query(string.Format("SUBJECT \"{0}\"", subject));
        }

        public static Query Text(string text)
        {
            return new Query(string.Format("TEXT \"{0}\"", text));
        }

        public static Query To(string to)
        {
            return new Query(string.Format("TO \"{0}\"", to));
        }

        public override string ToString()
        {
            return this.query;
        }

        public static Query UID(ISequence sequence)
        {
            return new Query(string.Format("UID {0}", sequence.ToString()));
        }

        public static Query UnAnswered()
        {
            return new Query("UNANSWERED");
        }

        public static Query UnDeleted()
        {
            return new Query("UNDELETED");
        }

        public static Query UnDraft()
        {
            return new Query("UNDRAFT");
        }

        public static Query UnFlagged()
        {
            return new Query("UNFLAGGED");
        }

        public static Query UnKeyword(Skycap.Net.Imap.Keyword keywords)
        {
            return new Query(string.Format("UNKEYWORD {0}", keywords));
        }

        public static Query Unseen()
        {
            return new Query("UNSEEN");
        }
    }
}

