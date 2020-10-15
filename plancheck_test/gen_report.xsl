﻿<?xml version="1.0"?>
<!--
////////////////////////////////////////////////////////////////////////////////
// gen_report.xsl
//
//  Helper for PlanQualityMetrics.cs, transforms generated report XML to HTML.
//  
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////
-->
<xsl:stylesheet
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0">

  <xsl:output method="html" />
  <!-- Possibilities: xml, html, text-->

  <!-- match the root element -->
  <xsl:template match="/">
    <!-- tell the XSLT process to keep digging through the document -->
    <xsl:apply-templates />
  </xsl:template>

  <!-- This template is applied against the <PlanQualityReport> element -->
  <xsl:template match="PlanQualityReport">
    <html>
      <!-- insert HTML element -->
      <head>
        <style type="text/css">
          body {
          margin-left: 5%;
          margin-right: 5%;
          font-family: sans-serif;}
          h1 { margin-left: -3%;}
          h2,h3,h4,h5,h6 { margin-left: -2%; }


          div.reportMeta {
          background-color: rgb(164,164,164);
          padding: 0.5em;
          border: none;
          width: 100%;
          }

          div.totalDoseCheck {
          background-color: red;
          padding: 0.5em;
          border: none;
          width: 100%;
          }

          div.planInfo {
          margin-top: 1em;
          padding-top: 1em;
          border-top: thin dotted }

          div.structureInfo {
          margin-top: 1em;
          padding-top: 1em;
          border-top: thin dotted }

          #planInfoTable
          {
          font-family:"Trebuchet MS", Arial, Helvetica, sans-serif;
          width:100%;
          border-collapse:collapse;
          }
          #planInfoTable td, #planInfoTable th
          {
          font-size:1em;
          border:1px solid #98bf21;
          padding:3px 7px 2px 7px;
          }
          #planInfoTable tr.alt td
          {
          color:#000000;
          background-color:#EAF2D3;
          }
          #planInfoTable th
          {
          text-align:left;
          }

          #beamsTable
          {
          font-family:"Trebuchet MS", Arial, Helvetica, sans-serif;
          width:100%;
          border:1.5px solid #98bf21;
          border-collapse:collapse;
          }
          #beamsTable td, #beamsTable th
          {
          font-size:1em;
          border:1px solid #98bf21;
          padding:3px 7px 2px 7px;
          }
          #beamsTable tr.alt td
          {
          color:#000000;
          background-color:#EAF2D3;
          }
          #beamsTable th
          {
          text-align:center;
          }

          <!-- ISW: Circles for traffic light added  -->
          .circleBase {
          border-radius: 50%;
          behavior: url(PIE.htc);
          /* width and height can be anything, as long as they're equal */
          }
          .green {
          width: 30px;
          height: 30px;
          background: #01DF01;
          border: 1px solid #000
          }
          .yellow {
          width: 30px;
          height: 30px;
          background: yellow;
          border: 1px solid #000
          }
          .red {
          width: 30px;
          height: 30px;
          background: red;
          border: 1px solid #000
          }

          #pqmTable
          {
          font-family:"Trebuchet MS", Arial, Helvetica, sans-serif;
          border:1.5px solid blue;
          border-collapse:collapse;
          }
          #pqmTable td, #pqmTable th
          {
          font-size:1em;
          border:1px solid blue;
          padding:3px 7px 2px 7px;
          }
          #pqmTable tr.alt td
          {
          color:#000000;
          background-color:#EAF2D3;
          }
          #pqmTable th
          {
          text-align:center;
          }
          <!-- ISW: Green color for PASS added  -->
          #pqmTable td.PASS
          {
          background-color:#01DF01;
          }
          #pqmTable td.WARN
          {
          background-color:yellow;
          }
          #pqmTable td.FAIL
          {
          background-color:red;
          }

          h3.target
          {
          background-color:light-green;
          }
          p.warning
          {
          color:orange;
          }
          p.error
          {
          color:red;
          }
        </style>
      </head>
      <body>
        <header>
          <hgroup>
            <h1>Plan Quality Report</h1>
            <h2>
              <!--<xsl:value-of select="concat(Patient/LastName, ', ', Patient/FirstName, ' (', Patient/@Id, ')' )"/>-->
              Patient ID : <xsl:value-of select=" Patient/@Id"/>
            </h2>
          </hgroup>
        </header>
        <div class="reportMeta">
          <p>
            Author : <xsl:value-of select="@userid"/><br/>
            Date : <xsl:value-of select="@created"/><br/>
            Eclipse Version : <xsl:value-of select="@eclipseVersion"/><br/>
            Script Version : <xsl:value-of select="@scriptVersion"/><br/>
            <!-- ISW: Adding the priority of the IMN in case it was given by the user -->
            <xsl:choose>
              <xsl:when test="@priorityIMN != ''">
                Prirority of IMN: <xsl:value-of select="@priorityIMN"/><br/>
              </xsl:when>
              <xsl:otherwise>
              </xsl:otherwise>
            </xsl:choose>
          </p>
        </div>
        <!-- ISW: Showing the  error text in case the total dose does not correspond to the diagnosis -->
        <xsl:choose>
          <xsl:when test="@totalDoseCheckText != ''">
            <div class="totalDoseCheck">
              <p>
                  <xsl:value-of select="@totalDoseCheckText"/> 
                <br/>
              </p>
            </div>
          </xsl:when>
          <xsl:otherwise>
          </xsl:otherwise>
        </xsl:choose>
        <div class="planInfo">
          <!-- tell XSLT to process the <PlanSetup> elements -->
          <xsl:apply-templates select="Patient/Courses/Course/PlanSetups/PlanSetup" />
        </div>
        <div class="structureInfo">
          <p>
            <h2>Structure Information</h2>
          </p>
          <!-- tell XSLT to process the <DoseStatistics/Structure> elements -->
          <xsl:apply-templates select="DoseStatistics/Structure[@present = 'True']">
            <xsl:with-param name="targetId" select="Patient/Courses/Course/PlanSetups/PlanSetup/TargetVolumeID"/>
          </xsl:apply-templates>
          <p>
            <br/>
            <br/>
            <h2>Plan check report:</h2>
          </p>
          <xsl:apply-templates select="PlanStatistics/Structure[@present = 'True']">
            <xsl:with-param name="targetId" select="Patient/Courses/Course/PlanSetups/PlanSetup/TargetVolumeID"/>
          </xsl:apply-templates>
          <p>
            <br/>
            <br/>
            <h2>Check manually:</h2>
          </p>
          <xsl:apply-templates select="Manual/Structure[@present = 'True']">
            <xsl:with-param name="targetId" select="Patient/Courses/Course/PlanSetups/PlanSetup/TargetVolumeID"/>
          </xsl:apply-templates>
        </div>
      </body>
    </html>
  </xsl:template>

  <!-- This template is applied against the <PlanSetup> elements -->
  <xsl:template match="Patient/Courses/Course/PlanSetups/PlanSetup">
    <h3>
      Plan : <xsl:value-of select="@Id"/>
    </h3>
    <table id="planInfoTable">
      <tr>
        <th># beams</th>
        <td>
          <xsl:value-of select="count(Beams/Beam)"/>
        </td>
      </tr>
      <tr>
        <th># Fx</th>
        <td>
          <xsl:value-of select="UniqueFractionation/NumberOfFractions"/>
        </td>
      </tr>
      <tr>
        <th>Total Prescribed Dose</th>
        <td>
          <xsl:value-of select="concat(number(round(TotalPrescribedDose/Dose*100) div 100.0), ' ',TotalPrescribedDose/Unit)"/>
        </td>
      </tr>
      <tr>
        <th>Prescribed Dose / Fx</th>
        <td>
          <xsl:value-of select="concat(number(round(UniqueFractionation/PrescribedDosePerFraction/Dose*100) div 100.0), ' ',UniqueFractionation/PrescribedDosePerFraction/Unit)"/>
        </td>
      </tr>
      <tr>
        <th>Planned Dose / Fx</th>
        <td>
          <xsl:value-of select="concat(number(round(UniqueFractionation/DosePerFractionInPrimaryRefPoint/Dose*100) div 100.0), ' ',UniqueFractionation/DosePerFractionInPrimaryRefPoint/Unit)"/>
        </td>
      </tr>
      <tr>
        <th>Plan Normalization Method</th>
        <td>
          <xsl:value-of select="PlanNormalizationMethod"/>
        </td>
      </tr>
      <tr>
        <th>Plan Normalization Value</th>
        <td>
          <xsl:value-of select="PlanNormalizationValue"/>
        </td>
      </tr>
      <tr>
        <th>Treatment Orientation</th>
        <td>
          <xsl:value-of select="TreatmentOrientation"/>
        </td>
      </tr>
      <tr>
        <th>Target</th>
        <td>
          <xsl:value-of select="TargetVolumeID"/>
        </td>
      </tr>
    </table>
    <p>
      Beams:<br/><br/>
    </p>
    <table  id="beamsTable" border="1">
      <tr>
        <th>#</th>
        <xsl:for-each select="Beams/Beam">
          <th>
            <xsl:value-of select="@Id"/>
          </th>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Beam Type</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:choose>
              <xsl:when test="IsSetupField = 'true'">Setup</xsl:when>
              <xsl:otherwise>Treatment</xsl:otherwise>
            </xsl:choose>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Technique</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:value-of select="Technique/@Id"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Energy</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:value-of select="EnergyModeDisplayName"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Dose Rate</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:value-of select="DoseRate"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>MU</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:value-of select="number(round(Meterset/Value*100) div 100.0)"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Dose</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:variable name="dose" select="number(round(FieldReferencePoints/FieldReferencePoint[IsPrimaryReferencePoint='true']/FieldDose/Dose*100) div 100.0)"/>
            <xsl:variable name="unit" select="FieldReferencePoints/FieldReferencePoint[IsPrimaryReferencePoint='true']/FieldDose/Unit"/>
            <xsl:value-of select="concat($dose, ' ', $unit)"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Gantry Start</th>
        <xsl:for-each select="BeamAndControlPoints/Beam">
          <td>
            <xsl:value-of select="ControlPoints/ControlPoint[1]/GantryAngle"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Gantry End</th>
        <xsl:for-each select="BeamAndControlPoints/Beam">
          <td>
            <xsl:value-of select="ControlPoints/ControlPoint[last()]/GantryAngle"/>
          </td>
        </xsl:for-each>
      </tr>
      <tr>
        <th>Gantry Direction</th>
        <xsl:for-each select="Beams/Beam">
          <td>
            <xsl:value-of select="GantryDirection"/>
          </td>
        </xsl:for-each>
      </tr>
    </table>
  </xsl:template>

  <!-- This template is applied against the <PlanSetup> elements -->
  <xsl:template match="DoseStatistics/Structure">
    <xsl:param name="targetId"/>
    <xsl:variable name="Id" select="@Id"/>
    <xsl:choose>
      <xsl:when test="$targetId != $Id">
        <h3>
          Structure : <xsl:value-of select="$Id"/>
        </h3>
      </xsl:when>
      <xsl:otherwise>
        <h3 class="target">
          Target Structure : <xsl:value-of select="$Id"/>
        </h3>
      </xsl:otherwise>
    </xsl:choose>
    <p>
      <xsl:variable name="volume" select="number(round(Volume*100) div 100.0)"/>
      <!-- xsl:variable name="volume" select="number(round(//Structures/Structure[@Id = $Id]/Volume*100) div 100.0)"/ -->
      Volume : <xsl:value-of select="concat($volume, ' cm3')"/><br/>
    </p>
    <xsl:choose>
      <xsl:when test="PQMs/PQM/Error">
        <p class="error">
          <xsl:value-of select="PQMs/PQM/Error[1]"/>
        </p>
      </xsl:when>
      <xsl:otherwise>
        <table id="pqmTable" border="1">
          <tr>
            <!-- ISW: Traffic Light added-->
            <th>Traffic Light</th>
            <th>Name</th>
            <th>Constraint</th>
            <th>Evaluation Point</th>
            <th>Calculated Value</th>
            <th>Goal</th>
            <!-- ISW: "Must" excluded from table output because we only have one dose limit, not a "Goal" and a "Must"
              <th>Must</th>
              -->
            <th>Evaluation</th>
          </tr>
          <!-- call generatePQMTableRowData function -->
          <xsl:call-template name="generatePQMTableRowData">
            <xsl:with-param name="pqmNode" select="PQMs/PQM[1]"/>
            <xsl:with-param	name="index" select="0"/>
          </xsl:call-template>
        </table>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- This template is applied against the <PlanSetup> elements -->
  <xsl:template match="PlanStatistics/Structure">
    <xsl:param name="targetId"/>
    <xsl:variable name="Id" select="@Id"/>
    <!--<xsl:choose>
      <xsl:when test="$targetId != $Id">
     
        <h3>
          Structure : <xsl:value-of select="$Id"/>
        </h3>
      </xsl:when>
      <xsl:otherwise>
        <h3 class="target">
          Target Structure : <xsl:value-of select="$Id"/>
        </h3>
      </xsl:otherwise>
    </xsl:choose>
    -->
    <!--
    <p>
      <xsl:variable name="volume" select="number(round(Volume*100) div 100.0)"/>
      xsl:variable name="volume" select="number(round(//Structures/Structure[@Id = $Id]/Volume*100) div 100.0)"/ 
      Volume : <xsl:value-of select="concat($volume, ' cm3')"/><br/>
    </p>
    -->
    <xsl:choose>
      <xsl:when test="PQMs/PQM/Error">
        <p class="error">
          <xsl:value-of select="PQMs/PQM/Error[1]"/>
        </p>
      </xsl:when>
      <xsl:otherwise>
        <table id="pqmTable" border="1">
          <col width="100px" />
          <col width="400px" />
          <col width="200px" />
          <col width="200px" />
          <col width="100px" />
          <tr>
            <!-- ISW: Traffic Light added-->
            <th>Traffic Light</th>
            <th>Name</th>
            <!-- <th>Type</th> -->
            <!--<th>Evaluation Point</th> -->
            <th>Calculated Value</th>
            <th>Goal</th>
            <!-- ISW: "Must" excluded from table output because we only have one dose limit, not a "Goal" and a "Must"
              <th>Must</th>
              -->
            <th>Evaluation</th>
          </tr>
          <!-- call generatePQMTableRowData function -->
          <xsl:call-template name="generateGeneralTableRowData">
            <xsl:with-param name="pqmNode" select="PQMs/PQM[1]"/>
            <xsl:with-param	name="index" select="0"/>
          </xsl:call-template>
        </table>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
    <!-- This template is applied against the <PlanSetup> elements -->
  <xsl:template match="Manual/Structure">
    <xsl:param name="targetId"/>
    <xsl:variable name="Id" select="@Id"/>
    <xsl:choose>
      <xsl:when test="$Id = ''">

      </xsl:when>
      <xsl:when test="$targetId != $Id">
     
        <h3>
          Part of check : <xsl:value-of select="$Id"/>
        </h3>
      </xsl:when>

      <xsl:otherwise>
        <h3 class="target">
          Target Structure : <xsl:value-of select="$Id"/>
        </h3>
      </xsl:otherwise>
    </xsl:choose>
    <!--
    <p>
      <xsl:variable name="volume" select="number(round(Volume*100) div 100.0)"/>
      xsl:variable name="volume" select="number(round(//Structures/Structure[@Id = $Id]/Volume*100) div 100.0)"/ 
      Volume : <xsl:value-of select="concat($volume, ' cm3')"/><br/>
    </p>
    -->
    <xsl:choose>
      <xsl:when test="PQMs/PQM/Error">
        <p class="error">
          <xsl:value-of select="PQMs/PQM/Error[1]"/>
        </p>
      </xsl:when>
      <xsl:otherwise>
        <table id="pqmTable" border="1">
          <col width="100px" />
          <col width="1500px" />
          <tr>
            <!-- ISW: Traffic Light added-->
            <th>Traffic Light</th>
            <th>Description</th>
            <!-- <th>Type</th> -->
            <!--<th>Evaluation Point</th> -->
            <!--<th>Calculated Value</th> -->
            <!--<th>Goal</th>-->
            <!-- ISW: "Must" excluded from table output because we only have one dose limit, not a "Goal" and a "Must"
              <th>Must</th>
              -->
            <!--<th>Evaluation</th>-->
          </tr>
          <!-- call generatePQMTableRowData function -->
          <xsl:call-template name="generateManualTableRowData">
            <xsl:with-param name="pqmNode" select="PQMs/PQM[1]"/>
            <xsl:with-param	name="index" select="0"/>
          </xsl:call-template>
        </table>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- template function generatePQMTableRowData-->
  <xsl:template name="generatePQMTableRowData">
    <xsl:param name="pqmNode" />
    <xsl:param name="index" />
    <xsl:variable name="nextIndex" select="$index+1"/>
    <xsl:choose>
      <xsl:when test="not($pqmNode)">
        <!-- No current node, just exit function-->
        <!--      !Exiting generatePointsTableRowData, index = <xsl:value-of select="$index"/> -->
      </xsl:when>
      <xsl:otherwise>
        <!-- figure out the units for the calculated value -->
        <xsl:variable name="volumeUnits">
          <xsl:choose>
            <xsl:when test="not($pqmNode/Volume)" />
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$pqmNode/Volume/@units = 'Relative'">%</xsl:when>
                <xsl:otherwise>cm3</xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="calculatedValueUnits">
          <xsl:choose>
            <xsl:when test="$pqmNode/DoseValue/@calculated = 'True'">
              <xsl:value-of select="$pqmNode/DoseValue/@units"/>
            </xsl:when>
            <xsl:when test="$pqmNode/Volume/@calculated = 'True'">
              <xsl:value-of select="$volumeUnits"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="resultColor">
          <xsl:choose>
            <xsl:when test="$pqmNode/Evaluate/Result/PFW = 'PASS'">green</xsl:when>
            <xsl:when test="$pqmNode/Evaluate/Result/PFW = 'WARN'">yellow</xsl:when>
            <xsl:otherwise>red</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <tr>
          <!-- ISW: Colors added to Traffic Light -->
          <xsl:choose>
            <xsl:when test="$pqmNode/@color = 'g'">
              <td>
                <center>
                  <div class="circleBase green"></div>
                  <!-- ISW: This is the green circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/@color = 'y'">
              <td>
                <center>
                  <div class="circleBase yellow"></div>
                  <!-- ISW: This is the yellow circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/@color = 'r'">
              <td>
                <center>
                  <div class="circleBase red"></div>
                  <!-- ISW: This is the red circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:otherwise>
              <td  style="background-color:white"></td>
            </xsl:otherwise>
          </xsl:choose>
          <td>
            <xsl:value-of select="$pqmNode/@name"/>
          </td>
          <td>
            <xsl:value-of select="$pqmNode/@type" disable-output-escaping="yes"/>
          </td>
          <xsl:choose>
            <xsl:when test="$pqmNode/DoseValue/@calculated = 'True'">
              <td>
                <xsl:value-of select="concat($pqmNode/Volume, ' ', $volumeUnits)"/>
              </td>
              <td>
                <xsl:value-of select="concat($pqmNode/DoseValue, ' ', $pqmNode/DoseValue/@units)"/>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/Volume/@calculated = 'True'">
              <td>
                <xsl:value-of select="concat($pqmNode/DoseValue, ' ', $pqmNode/DoseValue/@units)"/>
              </td>
              <td>
                <xsl:value-of select="concat($pqmNode/Volume, ' ', $volumeUnits)"/>
              </td>
            </xsl:when>
          </xsl:choose>
          <td>
            <xsl:value-of select="concat($pqmNode/Evaluate/Limit, ' ', $calculatedValueUnits)"/>
          </td>
          <!-- ISW: MaxLimit excluded from table output because we only have one dose limit, not a limit and a MaxLimit
          <td>
            <xsl:value-of select="concat($pqmNode/Evaluate/Result/MaxLimit, ' ', $calculatedValueUnits)"/>
          </td>-->
          <td class="{$pqmNode/Evaluate/Result/PFW}">
            <xsl:value-of select="$pqmNode/Evaluate/Result/PFW"/>
          </td>
        </tr>
        <!-- recursively call this function -->
        <xsl:call-template name="generatePQMTableRowData">
          <xsl:with-param name="pqmNode" select="$pqmNode/following-sibling::PQM[1]"/>
          <xsl:with-param	name="index" select="$nextIndex"/>
          <xsl:with-param name ="order" select ="ascending"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- template function generateGeneralTableRowData-->
  <xsl:template name="generateGeneralTableRowData">
    <xsl:param name="pqmNode" />
    <xsl:param name="index" />
    <xsl:variable name="nextIndex" select="$index+1"/>
    <xsl:choose>
      <xsl:when test="not($pqmNode)">
        <!-- No current node, just exit function-->
        <!--      !Exiting generatePointsTableRowData, index = <xsl:value-of select="$index"/> -->
      </xsl:when>
      <xsl:otherwise>
        <!-- figure out the units for the calculated value -->
        <xsl:variable name="volumeUnits">
          <xsl:choose>
            <xsl:when test="not($pqmNode/Volume)" />
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$pqmNode/Volume/@units = 'Relative'">%</xsl:when>
                <xsl:otherwise>cm3</xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="calculatedValueUnits">
          <xsl:choose>
            <xsl:when test="$pqmNode/DoseValue/@calculated = 'True'">
              <xsl:value-of select="$pqmNode/DoseValue/@units"/>
            </xsl:when>
            <xsl:when test="$pqmNode/Volume/@calculated = 'True'">
              <xsl:value-of select="$volumeUnits"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="resultColor">
          <xsl:choose>
            <xsl:when test="$pqmNode/Evaluate/Result/PFW = 'PASS'">green</xsl:when>
            <xsl:when test="$pqmNode/Evaluate/Result/PFW = 'WARN'">yellow</xsl:when>
            <xsl:otherwise>red</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        
        <tr>
          <!-- ISW: Colors added to Traffic Light -->
          <xsl:choose>
            <xsl:when test="$pqmNode/@color = 'g'">
              <td>
                <center>
                  <div class="circleBase green"></div>
                  <!-- ISW: This is the green circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/@color = 'y'">
              <td>
                <center>
                  <div class="circleBase yellow"></div>
                  <!-- ISW: This is the yellow circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/@color = 'r'">
              <td>
                <center>
                  <div class="circleBase red"></div>
                  <!-- ISW: This is the red circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:otherwise>
              <td  style="background-color:white"></td>
            </xsl:otherwise>
          </xsl:choose>
          <td>
            <h3>
            <xsl:value-of select="$pqmNode/@name"/>
            </h3>
          </td>
          <!--
          <td>
            <xsl:value-of select="$pqmNode/@type"/>
          </td>
          -->

          <xsl:choose>
            <xsl:when test="$pqmNode/DoseValue/@calculated = 'True'">
              <!--
              <td>
                <xsl:value-of select="concat($pqmNode/Volume, ' ', $volumeUnits)"/>
              </td>
              -->
              <td>
                <xsl:value-of select="concat($pqmNode/DoseValue, ' ', $pqmNode/DoseValue/@units)"/>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/Volume/@calculated = 'True'">
              <!--
              <td>
                <xsl:value-of select="concat($pqmNode/DoseValue, ' ', $pqmNode/DoseValue/@units)"/>
              </td>
              -->
              <td>
                <xsl:value-of select="concat($pqmNode/Volume, ' ', $volumeUnits)"/>
              </td>
            </xsl:when>
          </xsl:choose> 
          <td>
            <xsl:value-of select="concat($pqmNode/Evaluate/Limit, ' ', $calculatedValueUnits)"/>
          </td>
          <!-- ISW: MaxLimit excluded from table output because we only have one dose limit, not a limit and a MaxLimit
          <td>
            <xsl:value-of select="concat($pqmNode/Evaluate/Result/MaxLimit, ' ', $calculatedValueUnits)"/>
          </td>-->
          <td class="{$pqmNode/Evaluate/Result/PFW}">
            <xsl:value-of select="$pqmNode/Evaluate/Result/PFW"/>
          </td>
        </tr>
        <!-- recursively call this function -->
        <xsl:call-template name="generatePQMTableRowData">
          <xsl:with-param name="pqmNode" select="$pqmNode/following-sibling::PQM[1]"/>
          <xsl:with-param	name="index" select="$nextIndex"/>
          <xsl:with-param name ="order" select ="ascending"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
    <!-- template function generateManualTableRowData-->
  <xsl:template name="generateManualTableRowData">
    <xsl:param name="pqmNode" />
    <xsl:param name="index" />
    <xsl:variable name="nextIndex" select="$index+1"/>
    <xsl:choose>
      <xsl:when test="not($pqmNode)">
        <!-- No current node, just exit function-->
        <!--      !Exiting generatePointsTableRowData, index = <xsl:value-of select="$index"/> -->
      </xsl:when>
      <xsl:otherwise>
        <!-- figure out the units for the calculated value -->
        
        <tr>
          <!-- ISW: Colors added to Traffic Light -->
          <xsl:choose>
            <xsl:when test="$pqmNode/@color = 'g'">
              <td>
                <center>
                  <div class="circleBase green"></div>
                  <!-- ISW: This is the green circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/@color = 'y'">
              <td>
                <center>
                  <div class="circleBase yellow"></div>
                  <!-- ISW: This is the yellow circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:when test="$pqmNode/@color = 'r'">
              <td>
                <center>
                  <div class="circleBase red"></div>
                  <!-- ISW: This is the red circle-->
                </center>
              </td>
            </xsl:when>
            <xsl:otherwise>
              <td  style="background-color:white"></td>
            </xsl:otherwise>
          </xsl:choose>
          <td>
            <h3>
              <center>
            <xsl:value-of select="$pqmNode/@text"/>
                </center>
            </h3>
          </td>
        </tr>
        <!-- recursively call this function -->
        <xsl:call-template name="generatePQMTableRowData">
          <xsl:with-param name="pqmNode" select="$pqmNode/following-sibling::PQM[1]"/>
          <xsl:with-param	name="index" select="$nextIndex"/>
          <xsl:with-param name ="order" select ="ascending"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- 
	template function generateBeamTableColumnData
-->
  <xsl:template name="generateBeamTableColumnData">
    <xsl:param name="beamNode" />
    <xsl:param name="xpath" />
    <xsl:param name="index" />
    <xsl:variable name="nextIndex" select="$index+1"/>
    <xsl:choose>
      <xsl:when test="not($beamNode)">
        <!-- No current node, just exit function-->
        <!--      !Exiting generateBeamTableColumnData, index = <xsl:value-of select="$index"/> -->
      </xsl:when>
      <xsl:otherwise>
        <td>
          <xsl:value-of select="$xpath"/>
        </td>
        <!-- recursively call this function -->
        <xsl:call-template name="generateBeamTableColumnData">
          <xsl:with-param name="beamNode" select="$beamNode/following-sibling::Beam[1]"/>
          <xsl:with-param	name="xpath" select="$xpath"/>
          <xsl:with-param	name="index" select="$nextIndex"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>