﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System.Text;
using Autodesk.Revit.DB.Electrical;
using System.Security.Cryptography;
using System.Xml.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;
//Include other Namespaces based on your needs

namespace MHQuickSelect20230916
{
    public class MHTraverse
    {
        List<ElementId> m_lVisited, m_lSelectedElts;
        Document m_pDoc;
        const int MAX_LOOPS = 50;
        public MHTraverse(UIApplication pUIApp, Reference obj)
        {
            InitializeLists();
            m_pDoc = pUIApp.ActiveUIDocument.Document;
            ElementId selId = obj.ElementId;
            TaskDialog.Show("Selection: ", selId.ToString());
            Element elemRun = m_pDoc.GetElement(selId);
            List<ElementId> listConduitsinRun = GetConduitsinRun(elemRun);
            foreach(ElementId id in listConduitsinRun)
            {
                RunStepThroughElements(id);
                m_lVisited.Add(id);
            }
            List<ElementId> m_lVisitedClean = m_lVisited.Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            foreach (ElementId el in m_lVisitedClean)
            {
                sb.Append(el.ToString() + "\n");
            }
            TaskDialog.Show("m_lVisited: ", sb.ToString() + "\n" + "Count is: " + m_lVisitedClean.Count.ToString());

        }

        public void InitializeLists()
        {
            m_lSelectedElts = new List<ElementId>();
            m_lVisited = new List<ElementId>();
        }
        public void RunStepThroughElements(ElementId eid)
        {
            Element elem = m_pDoc.GetElement(eid);
            var connSet = GetConnectors(elem);

            foreach ( Connector conn in connSet ) 
            {
                Element elemNext = GetNextConnectedElement(conn, elem, m_lVisited);
                if (elemNext == null) continue;
                //TaskDialog.Show("elemNext: ", elemNext.Id.ToString());
                m_lVisited.Add(elemNext.Id);
                //Do what you want with the Elements here
                //Get data, etc.
                //And make sure we mark it as visited 
                //so we don't come back to it.
            }
        }
        public ConnectorSet GetConnectors(Element pElt)
        {
            if (pElt is Conduit) return ((Conduit)pElt)?.ConnectorManager?.Connectors;
            if (pElt is FamilyInstance) return ((FamilyInstance)pElt)?.MEPModel?.ConnectorManager?.Connectors;
            //Add other statements to accomodate your 
            //needs based on different element types

            return null;
        }
        /// <summary>
        /// Return true if we want to look at the 
        /// next Connector, and false if the current 
        /// Connector is desired
        /// </summary>
        /// <param name="pConn">The Current Connector</param>
        /// <param name="pPrevElement">The previous Element the Connector came from</param>
        /// <param name="lVisited">A List of visited Elements (by their Id's)</param>
        /// <returns></returns>
        public bool NextConnector(Connector pConn, Element pPrevElement, List<ElementId> lVisited)
        {
            return (pConn.Owner == pPrevElement) ||
                   (pConn.Owner.Id == pPrevElement.Id) ||
                   (pConn.Domain != Domain.DomainCableTrayConduit) || (lVisited.Contains(pConn.Owner.Id))  //Change Domain based on need
                   ;
        }
        /// <summary>
        /// Gets the Element the Connector is connected 
        /// to if it has not been visited before. Currently 
        /// set to work with Pipe and FamilyInstance types
        /// - change types to accomodate
        /// </summary>
        /// <param name="pConn">The Connector from which we want to grab the connected Element</param>
        /// <param name="pPrevElem">The Element from which we are coming from</param>
        /// <param name="lVistied">List of visited Elements (by their Id's)</param>
        /// <returns></returns>
        public Element GetNextConnectedElement(Connector pConn, Element pPrevElem, List<ElementId> lVisited)
        {
            foreach (Connector pRef in pConn.AllRefs)
            {
                if (NextConnector(pRef, pPrevElem, lVisited)) continue;
                return pRef.Owner;
            }
            return null;
        }

        public bool CheckIfConnected (Element elem)
        {
            bool result = true;
            var connSet = GetConnectors(elem);
            foreach(Connector conn in connSet) 
            {
                result = conn.IsConnected;
            }
            return result;
        }

        public double CalcAngleSum(Element elem)
        {
            double angleSum = 0;
            if (elem is FamilyInstance)
            {
                Parameter parameter1 = elem.LookupParameter("Angle");
                if (elem is FamilyInstance) angleSum += ConvertAngleToDouble(parameter1);
            }
            return angleSum;
        }
        public double ConvertAngleToDouble(Parameter param)
        {
            return Convert.ToDouble(param.AsValueString().Remove(param.AsValueString().Length - 1, 1));
        }
        public List<ElementId> GetConduitsinRun(Element elem)
        {
            List<ElementId> list = new List<ElementId>();   
            IList<Element> condCollector = new FilteredElementCollector(m_pDoc).OfClass(typeof(Conduit)).ToElements();
            if (elem is FamilyInstance)
            {
                var connSet = GetConnectors(elem);
                foreach (Connector conn in connSet)
                {
                    Element elemNext = GetNextConnectedElement(conn, elem, m_lVisited);
                    elem = elemNext;
                }
            }
            //This code only works if a Conduit is selected (not a fitting)
            StringBuilder sb = new StringBuilder();
            foreach(Element e in condCollector)
            {
                if (e is Conduit && ((Conduit)e)?.RunId == ((Conduit)elem)?.RunId)
                {
                    list.Add(e.Id);
                    sb.Append(e.Id.ToString() + "\n");
                }
            }
            TaskDialog.Show("Conduit Run id: ", sb.ToString() + "\n" + "Count is: " + list.Count.ToString());
            return list;
        }
    }
    
}
