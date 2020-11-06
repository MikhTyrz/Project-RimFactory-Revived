﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;
namespace ProjectRimFactory {
    /****************************************************************
     * Modders can extend defs by use of DefModExtensions. But there
     * are no such option extensions for GraphicData in the XML, and
     * sometimes you just need more data.
     * We can overcome that limitation! GraphicData contains exactly
     * one string: the texPath. We can interpret that as XML and use
     * RimWorld's load-from-xml functions to get the extra data!
     * Win!
     * (Note that we have to encode '<' as '[' Well, anyone who uses
     *  '[' in a filename gets what's coming to them?)
     * USAGE NOTE:
     *   public override void Init(GraphicRequest req) {
     *     var extraData = GraphicExtraData.Extract(req, out this.path, out req2, TODO);
     *     // req2 is new GraphicRequest
     *     // req2.path and req2.graphicData.texPath are correct
     *     // use req2 for init, do all init in ExtraInit:
     *     ExtraInit(req2, extraData); // extraData may be null
     *   }
     * A parent can initialize its children by modifying `req` as
     * above and then calling child.Init(req2); followed by
     * child.ExtraInit(req2, extraData);
     * If the child doesn't do anything in Init() (preferred), it 
     * can simply call child.ExtraInit(req2, extraData);
     *
     * Final note: I still don't fully understand everything I
     * am doing, so ...good luck, god speed, etc?
     ***************************************************************/
    public class GraphicExtraData {
        public Vector3? arrowDrawOffset;     // Vector3? so we can
        public Vector3? arrowEastDrawOffset; // test against `null`
        public Vector3? arrowWestDrawOffset; // and only update if
        public Vector3? arrowNorthDrawOffset;  // actually changed
        public Vector3? arrowSouthDrawOffset;  // in def's XML.
        public string texPath = null; // actual texPath
        public string texPath2 = null;  // splitter building, wall edges, whatever?
        public List<ThingDef> specialLinkDefs;
        public string inputString = null;

        public static GraphicExtraData Extract(GraphicRequest req, 
                                           out GraphicRequest outReq,
                                           bool removeExtraFromReq=false) {
            outReq = CopyGraphicRequest(req);
            if (req.path[0] == '[') {
                GraphicExtraData extraData = null;
                try {
                    var helperDoc = new System.Xml.XmlDocument();
                    helperDoc.LoadXml(req.path.Replace('[', '<').Replace(']', '>'));
                    extraData = DirectXmlToObject.ObjectFromXml<GraphicExtraData>(
                                                   helperDoc.DocumentElement, false);
                } catch (Exception e) {
                    Log.Error("GraphicExtraData was unable to extract XML from \"" + req.path + 
                    "\"; Exception: "+e);
                    return null;
                }
                extraData.inputString = req.path;
                if (removeExtraFromReq) {
                    outReq.graphicData.texPath = extraData.texPath;
                    outReq.path = extraData.texPath;
                }
                return extraData;
            }
            return null;
        }
        //no idea if this is necessary, but *it works* 
        //  - "it works" is a general theme here
        public static GraphicRequest CopyGraphicRequest(GraphicRequest req, string newTexPath = null) {
            GraphicData gData = new GraphicData();
            gData.CopyFrom(req.graphicData);
            var gr = new GraphicRequest(gData.graphicClass, gData.texPath, req.shader,
                  req.drawSize, req.color, req.colorTwo, gData, req.renderQueue,
                  req.shaderParameters);
            if (newTexPath != null) {
                gr.path = newTexPath;
                gr.graphicData.texPath = newTexPath;
            }
            return gr;
        }
    }
    public interface IHaveGraphicExtraData {
        void ExtraInit(GraphicRequest req, GraphicExtraData extraData);
    }
}
