﻿/* 
 * Copyright (C) 2014-2016 John Torjo
 *
 * This file is part of LogWizard
 *
 * LogWizard is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LogWizard is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact john.code@torjo.com 
 *
 * **** Get Latest version at https://github.com/jtorjo/logwizard **** 
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogWizard;

namespace lw_common {
    // information about the UI of LogWizard - location, toggles, etc.
    public class ui_info {
        public int left, top, width, height = 0;
        public bool maximized = false;

        public bool was_set_at_least_once {
            get { return width > 0; }
        }

        public bool show_left_pane {
            get { return show_left_pane_; }
        }

        public bool show_filter {
            get { return show_filter_; }
            set {
                show_filter_ = value;
                if (show_filter_) {
                    show_notes_ = false;
                    show_categories_ = false;
                    show_left_pane_ = true;
                } else
                    show_left_pane_ = false;
            }
        }

        public bool show_notes {
            get { return show_notes_; }
            set {
                show_notes_ = value;
                if (show_notes_) {
                    show_filter_ = false;
                    show_categories_ = false;
                    show_left_pane_ = true;
                } else
                    show_left_pane_ = false;
            }
        }

        public bool show_categories {
            get { return show_categories_; }
            set {
                show_categories_ = value;
                if (show_categories_) {
                    show_notes_ = false;
                    show_filter_ = false;
                    show_left_pane_ = true;
                } else
                    show_left_pane_ = false;
            }
        }

        // if true, we show the status, but only for a short while (probably to show an error)
        // we DO NOT persist this
        public bool temporarily_show_status = false;

        // show/hide toggles
        public bool show_source = false;
        public bool show_fulllog = false;

        public bool show_current_view = true;
        public bool show_header = true;
        public bool show_tabs = true;
        public bool show_title = true;
        public bool topmost = false;
        public bool show_status = true;

        public bool show_details = false;

        public string selected_view = "";
        // the name of the last opened log
        public string last_log_guid = ""; 
        public int selected_row_idx = -1;
        public int full_log_splitter_pos = -1;
        public int left_pane_pos = -1;

        private bool show_filter_ = false;
        private bool show_left_pane_ = false;
        private bool show_notes_ = false;
        private bool show_categories_ = false;

        public int description_splitter_pos = -1;

        // IMPORTANT: Update Toggles UI when adding stuff here!

        // contains per-view settings
        public class view_info {
            // 1.3.28+ if true, we're showing the full log (the rows that don't match the filter are shown in gray)
            internal bool show_full_log_ = false;

            public bool show_full_log {
                get { return show_full_log_; }
            }

            internal view_info() {
            }

            public view_info(bool show_full_log) {
                show_full_log_ = show_full_log;
            }

            internal string to_string() {
                // 3 = the version - just in case later on i want to change how i save this
                return "3/" + (show_full_log_ ? "1" : "0") ;
            }

            internal static view_info from_string(string s) {
                view_info vi = new view_info();
                if ( s == "")
                    return vi;
                var from = s.Split('/');
                if (from[0] != "3")
                    // older version
                    return vi;

                Debug.Assert(from.Length == 2);
                vi.show_full_log_ = from[1] != "0";
                return vi;
            }

        }
        private readonly view_info default_view_info_  = new view_info();

        // FIXME (minor) I should ignore views that have been deleted
        private Dictionary<string, view_info> views_ = new Dictionary<string, view_info>(); 

        public void copy_from(ui_info other) {
            left = other.left;
            top = other.top;
            width = other.width;
            height = other.height;

            show_filter_ = other.show_filter_;
            show_source = other.show_source;
            show_fulllog = other.show_fulllog;

            show_current_view = other.show_current_view;
            show_header = other.show_header;
            show_tabs = other.show_tabs;
            show_title = other.show_title;
            topmost = other.topmost;
            show_details = other.show_details;
            show_status = other.show_status;
            last_log_guid = other.last_log_guid;

            selected_view = other.selected_view;
            selected_row_idx = other.selected_row_idx;
            full_log_splitter_pos = other.full_log_splitter_pos;
            description_splitter_pos = other.description_splitter_pos;
            left_pane_pos = other.left_pane_pos;

            show_left_pane_ = other.show_left_pane_;
            show_notes_ = other.show_notes_;
            show_categories_ = other.show_categories_;
            // ... get a copy
            views_ = other.views_.ToDictionary(x => x.Key, x => new view_info(x.Value.show_full_log) );
        }

        // note: this is read-only
        public view_info view(string name) {
            if (views_.ContainsKey(name))
                return views_[name];
            else 
                return default_view_info_;            
        }

        public void view(string name, view_info vi) {
            if (!views_.ContainsKey(name))
                views_.Add(name, vi);
            else
                views_[name] = vi;            
        }

        public void rename_view(string old_name, string new_name) {
            if (!views_.ContainsKey(old_name))
                // we don't have any per-view info for this view, so we don't really care
                return;

            views_.Add( new_name, views_[old_name]);
            views_.Remove(old_name);
        }

        private void load_save(bool load, string prefix) {
            app.load_save(load, ref left, prefix + ".left", -1);
            app.load_save(load, ref top, prefix + ".top", -1);
            app.load_save(load, ref width, prefix + ".width", -1);
            app.load_save(load, ref height, prefix + ".height", -1);
            app.load_save(load, ref maximized, prefix + ".maximized", false);

            app.load_save(load, ref show_filter_, prefix + ".show_filter", false);
            app.load_save(load, ref show_source, prefix + ".show_source", false);
            app.load_save(load, ref show_fulllog, prefix + ".show_fulllog", false);

            app.load_save(load, ref show_current_view, prefix + ".show_current_view", true);
            app.load_save(load, ref show_header, prefix + ".show_header", true);
            app.load_save(load, ref show_status, prefix + ".show_status", true);
            app.load_save(load, ref show_tabs, prefix + ".show_tabs", true);
            app.load_save(load, ref show_title, prefix + ".show_title", true);
            app.load_save(load, ref show_details, prefix + ".show_details", false);
            app.load_save(load, ref topmost, prefix + ".topmost", false);

            app.load_save(load, ref selected_view, prefix + ".selected_view", selected_view);
            app.load_save(load, ref last_log_guid, prefix + ".last_log_guid");
            app.load_save(load, ref selected_row_idx, prefix + ".selected_row_idx", -1);
            app.load_save(load, ref full_log_splitter_pos, prefix + ".full_log_splitter_pos", -1);
            app.load_save(load, ref left_pane_pos, prefix + ".left_pane_pos", -1);

            app.load_save(load, ref show_left_pane_, prefix + ".show_left_pane", false);
            app.load_save(load, ref show_notes_, prefix + ".show_notes", false);
            app.load_save(load, ref show_categories_, prefix + ".show_categories", false);

            app.load_save(load, ref description_splitter_pos, prefix + ".description_splitter_pos", -1);

            load_save_view_info(load, prefix + ".view_info");
        }

        private void load_save_view_info(bool load, string prefix) {
            if (load) {
                Dictionary<string,string> view = new Dictionary<string, string>();
                app.load_save(load, ref view, prefix);
                views_.Clear();
                foreach ( var kv in view)
                    views_.Add( kv.Key, view_info.from_string(kv.Value));
            } else {
                var view = views_.ToDictionary(x => x.Key, x => x.Value.to_string());
                app.load_save(load, ref view, prefix);
            }
        }

        public void load(string prefix) {
            load_save(true, prefix);
        }

        public void save(string prefix) {
            load_save(false, prefix);
        }
    }
}
