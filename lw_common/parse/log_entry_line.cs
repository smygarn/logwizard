﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lw_common.parse {
    // represents a "conceptual" line - contains a log entry, but it could be on several lines
    // however, we will compact it on a single line
    class log_entry_line {
        private string entry_ = "";
        private readonly Dictionary<string ,int> infos_ = new Dictionary<string, int>();
        // .. so i know the order they were added
        private readonly List<string> names_ = new List<string>(); 


        // performs an analysis of the text, and then adds it. looks for common patterns, such as whether it's a timestamp or not
        //
        // note: this can end up adding multiple parts
        public void analyze_and_add(string name, string value) {
            bool is_timestamp = name.Contains("timestamp");
            if (!is_timestamp)
                // check for possible timestamp entries
                is_timestamp = util.is_timestamp_fast(value);

            if ( is_timestamp)
                add_timestamp_to_entry(name, value);
            else 
                add(name, value);
        }

        public void add(string name, string value) {
            infos_.Add(name, entry_.Length);
            names_.Add(name);
            entry_ += value;
        }

        private void add_timestamp_to_entry(string name, string text) {
            var stamp = util.split_timestamp(text);
            add("date", stamp.Item1);
            add("time", stamp.Item2);
        }


        // it's basically something to be appended to last entry
        public void append_to_last(string value) {
            entry_ += value;
        }

        public override string ToString() {
            return entry_;
        }

        public int entry_count {
            get { return names_.Count; }
        }

        public List<string> names {
            get { return names_; }
        } 

        public Tuple<int, int>[] idx_in_line(aliases aliases) {
            var idx = new Tuple<int, int>[(int) info_type.max];
            for (int i = 0; i < idx.Length; ++i)
                idx[i] = new Tuple<int, int>(-1, -1);

            var sorted = infos_.OrderBy(x => x.Value).Select(x => new Tuple<int, int>((int) aliases.to_info_type(x.Key, names_) , x.Value)).ToList();

            for (int i = 0; i < sorted.Count; ++i) {
                int len = i < sorted.Count - 1 ? sorted[i + 1].Item2 - sorted[i].Item2 : -1;
                int cur_idx = sorted[i].Item1;
                // we can have too many columns - we will ignore the last ones
                if ( cur_idx < idx.Length)
                    idx[sorted[i].Item1] = new Tuple<int, int>(sorted[i].Item2, len);
            }

            return idx;
        }

    }
}