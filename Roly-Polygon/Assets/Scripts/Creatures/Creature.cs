using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class EdgeTransitionInfo {
    public float position;//0.0 to 1.0, position along edge
    public float angle;//angle as measured clockwise from the perpendicular into the polygon
    public float slope;//signed angle between the normals of the old polygon and new polygon (positive is uphill, negative is downhill)
    public EdgeTransitionInfo(float position, float angle, float slope) {
        this.position = position;
        this.angle = angle;
        this.slope = slope;
    }
}
public class EdgeTransitionHeading {
    public float position;//0.0 to 1.0, position along edge
    public float angle;//angle as measured clockwise from the perpendicular into the polygon
    public EdgeTransitionHeading(float position, float angle) {
        this.position = position;
        this.angle = angle;
    }
}
public class EdgeTransitionRule {
    public Func<EdgeTransitionInfo, EdgeTransitionHeading> rule;
    public EdgeTransitionRule(Func<EdgeTransitionInfo, EdgeTransitionHeading> rule) {
        this.rule = rule;
    }
    public static EdgeTransitionHeading passThrough(EdgeTransitionInfo transition) {
        return new EdgeTransitionHeading(transition.position, transition.angle);
    }
    public static EdgeTransitionHeading reflect(EdgeTransitionInfo transition) {
        return new EdgeTransitionHeading(transition.position, -transition.angle);
    }
    public static EdgeTransitionHeading normal(EdgeTransitionInfo transition) {
        return new EdgeTransitionHeading(transition.position, 0);
    }
    public static EdgeTransitionHeading midpoint(EdgeTransitionInfo transition) {
        return new EdgeTransitionHeading(0.5f, transition.angle);
    }
    public static EdgeTransitionHeading random(EdgeTransitionInfo transition) {
        return new EdgeTransitionHeading(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(-180f, 180f));
    }
}
public class Creature : MonoBehaviour {
    public EdgeTransitionRule edgeTransitionRule;
    private void Awake() {
        edgeTransitionRule = new EdgeTransitionRule(EdgeTransitionRule.reflect);
    }
}